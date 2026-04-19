using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SJMagic.Services
{
    public class MediaService
    {
        public string FfmpegPath { get; set; } = "ffmpeg.exe"; // Default to PATH

        public async Task<bool> ConvertToEditorFriendly(string inputPath, string outputPath, Action<string> logCallback)
        {
            if (!File.Exists(inputPath)) return false;

            // Editor-friendly parameters: -c:v libx264 -crf 17 -g 1 -pix_fmt yuv420p
            // -g 1: Intra-frame only (every frame is a keyframe)
            // -crf 17: High quality
            string arguments = $"-i \"{inputPath}\" -c:v libx264 -crf 17 -g 1 -pix_fmt yuv420p -c:a copy \"{outputPath}\" -y";

            logCallback?.Invoke($"변환 시작: {Path.GetFileName(inputPath)} -> {Path.GetFileName(outputPath)}");
            logCallback?.Invoke($"FFmpeg 실행 중...");

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
                    
                    // FFmpeg logs to StandardError
                    string error = await process.StandardError.ReadToEndAsync();
                    
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        logCallback?.Invoke("변환 성공!");
                        return true;
                    }
                    else
                    {
                        logCallback?.Invoke($"변환 실패 (ExitCode: {process.ExitCode})");
                        logCallback?.Invoke($"오류 메시지: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"오류 발생: {ex.Message}");
                return false;
            }
        }
    }
}
