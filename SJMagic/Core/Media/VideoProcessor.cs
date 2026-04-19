using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SJMagic.Core.Media
{
    /// <summary>
    /// 동영상 편집 및 처리를 담당하는 핵심 클래스입니다.
    /// FFmpeg 도구를 사용하여 실제 작업을 수행하며, 각 작업의 로직이 모듈화되어 있습니다.
    /// </summary>
    public class VideoProcessor
    {
        // 내장된 FFmpeg 경로 (Libraries/FFmpeg 폴더)
        public string FfmpegPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries", "FFmpeg", "ffmpeg.exe");

        /// <summary>
        /// 학습용 팁: '동영상 쪼개기'의 동작 원리
        /// 1. 원본 파일의 정보를 읽어옵니다.
        /// 2. 재인코딩 없이(Copy) 조각내기 설정(Segment)을 적용합니다.
        /// 3. 지정된 시간(단위)마다 새로운 파일을 생성합니다.
        /// </summary>
        public async Task<bool> SplitVideoAsync(string inputPath, string outputDir, int intervalSeconds, Action<string> logCallback)
        {
            if (!File.Exists(inputPath)) return false;
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            
            // 파일명 패턴 설정 (예: video_part001.mp4)
            string outputPattern = Path.Combine(outputDir, $"{fileNameWithoutExt}_part%03d{extension}");

            // [학습 포인트] 빌더를 사용하여 명령어를 조립합니다.
            // 코드가 한 줄씩 어떤 옵션을 추가하는지 명확히 볼 수 있습니다.
            var cmd = new FFmpegCommandBuilder()
                .Input(inputPath)
                .CopyStream()               // 재인코딩 안함 (속도 중시)
                .SplitIntoSegments(intervalSeconds) // 시간 단위 쪼개기
                .Output(outputPattern)
                .Overwrite()
                .Build();

            logCallback?.Invoke($"작업 시작: {Path.GetFileName(inputPath)}을(를) {intervalSeconds}초 단위로 쪼갭니다.");
            return await ExecuteAsync(cmd, logCallback);
        }

        /// <summary>
        /// 학습용 팁: 'Editor-Friendly 변환'의 동작 원리
        /// - 일반 영상은 프레임 사이의 '차이'만 저장해서 용량을 줄입니다.
        /// - 이 경우 편집기에서 뒤로가기나 미세 조정 시 컴퓨터가 계산을 많이 해야 합니다.
        /// - 'All-Intra' 방식은 모든 프레임을 사진처럼 통째로 저장하여 편집 시 속도가 매우 빨라집니다.
        /// </summary>
        public async Task<bool> ConvertToEditorFriendlyAsync(string inputPath, string outputPath, Action<string> logCallback)
        {
            var cmd = new FFmpegCommandBuilder()
                .Input(inputPath)
                .VideoCodec("libx264") // 가장 호환성 좋은 H.264 코덱 사용
                .Quality(17)           // 고화질 설정
                .ForceAllIntra()       // 모든 프레임을 독립 프레임으로 (편집 최적화)
                .Output(outputPath)
                .Overwrite()
                .Build();

            logCallback?.Invoke($"변환 시작: {Path.GetFileName(inputPath)} -> {Path.GetFileName(outputPath)}");
            return await ExecuteAsync(cmd, logCallback);
        }

        /// <summary>
        /// 실제로 FFmpeg 프로세스를 실행하고 로그를 기록하는 공통 메서드입니다.
        /// </summary>
        private async Task<bool> ExecuteAsync(string arguments, Action<string> logCallback)
        {
            logCallback?.Invoke($"실행 명령어: ffmpeg {arguments}");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = FfmpegPath,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();
                    
                    // FFmpeg은 작업 진행 상황을 StandardError 스트림으로 출력합니다.
                    string output = await process.StandardError.ReadToEndAsync();
                    
                    // .NET 4.8 등 구형 환경에서는 WaitForExitAsync 대신 Task.Run 사용
                    await Task.Run(() => process.WaitForExit());

                    if (process.ExitCode == 0)
                    {
                        logCallback?.Invoke("성공적으로 완료되었습니다.");
                        return true;
                    }
                    else
                    {
                        logCallback?.Invoke($"오류 발생 (종료 코드: {process.ExitCode})");
                        logCallback?.Invoke($"내용: {output}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"시스템 오류: {ex.Message}");
                if (ex is System.ComponentModel.Win32Exception)
                {
                    logCallback?.Invoke("도움말: FFmpeg.exe 파일을 찾을 수 없습니다.");
                    logCallback?.Invoke("- 'SplitVideo'와 'Convert' 기능을 사용하려면 SJMagic/Libraries/FFmpeg/ 폴더 안에 ffmpeg.exe 파일을 직접 넣어주어야 합니다.");
                    logCallback?.Invoke("- 또는 시스템 PATH에 ffmpeg이 등록되어 있어야 합니다.");
                }
                return false;
            }
        }
    }
}
