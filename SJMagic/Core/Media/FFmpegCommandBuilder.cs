using System;
using System.Collections.Generic;
using System.Text;

namespace SJMagic.Core.Media
{
    /// <summary>
    /// FFmpeg 명령어를 사람이 읽기 쉬운 형태로 조립해주는 클래스입니다.
    /// 구체적인 코덱 옵션의 의미를 학습할 수 있도록 주석이 포함되어 있습니다.
    /// </summary>
    public class FFmpegCommandBuilder
    {
        private readonly List<string> _inputs = new List<string>();
        private readonly List<string> _options = new List<string>();
        private string _output;

        public FFmpegCommandBuilder Input(string path)
        {
            _inputs.Add($"-i \"{path}\"");
            return this;
        }

        /// <summary>
        /// 스트림 복사 모드 설정을 추가합니다. (-c copy)
        /// 인코딩을 다시 하지 않기 때문에 화질 저하가 없고 속도가 매우 빠릅니다.
        /// </summary>
        public FFmpegCommandBuilder CopyStream()
        {
            _options.Add("-c copy");
            return this;
        }

        /// <summary>
        /// 모든 프레임을 키프레임(Intra-frame)으로 설정합니다. (-g 1)
        /// [학습 포인트]
        /// - 보통의 영상은 용량을 줄이기 위해 이전 프레임과의 차이점만 기록합니다.
        /// - 하지만 편집(Editing) 시에는 어느 지점에서나 전후 이동이 자유로워야 하므로, 
        ///   모든 프레임이 독립적인 정보를 갖는 GOP(Group Of Pictures) 크기 1 설정을 사용합니다.
        /// </summary>
        public FFmpegCommandBuilder ForceAllIntra()
        {
            _options.Add("-g 1");
            return this;
        }

        /// <summary>
        /// 비디오 코덱을 설정합니다. (-c:v libx264)
        /// H.264는 전 세계에서 가장 널리 쓰이는 표준 코덱입니다.
        /// </summary>
        public FFmpegCommandBuilder VideoCodec(string codec = "libx264")
        {
            _options.Add($"-c:v {codec}");
            return this;
        }

        /// <summary>
        /// 화질(분해능)을 설정합니다. (-crf 17)
        /// [학습 포인트]
        /// - CRF(Constant Rate Factor)는 0~51 사이의 값을 가집니다.
        /// - 값이 낮을수록 화질이 좋고 용량이 큽니다. (17~18은 시각적으로 무손실에 가깝습니다.)
        /// </summary>
        public FFmpegCommandBuilder Quality(int crf = 17)
        {
            _options.Add($"-crf {crf}");
            return this;
        }

        /// <summary>
        /// 동영상 쪼개기(Segment) 설정을 추가합니다.
        /// [학습 포인트]
        /// - '-f segment': 동영상을 여러 조각으로 나누는 포맷을 사용함을 알립니다.
        /// - '-segment_time': 조각당 시간을 초 단위로 설정합니다.
        /// - '-reset_timestamps 1': 각 조각의 시작 시간을 0으로 초기화하여 편집기에서 읽기 편하게 만듭니다.
        /// </summary>
        public FFmpegCommandBuilder SplitIntoSegments(int seconds)
        {
            _options.Add("-f segment");
            _options.Add($"-segment_time {seconds}");
            _options.Add("-reset_timestamps 1");
            return this;
        }

        public FFmpegCommandBuilder Output(string path)
        {
            _output = $"\"{path}\"";
            return this;
        }

        /// <summary>
        /// 기존 파일이 있을 경우 묻지 않고 덮어씁니다. (-y)
        /// </summary>
        public FFmpegCommandBuilder Overwrite()
        {
            _options.Add("-y");
            return this;
        }

        public string Build()
        {
            var sb = new StringBuilder();
            
            // 1. 입력 파일들 추가
            foreach (var input in _inputs) sb.Append(input).Append(" ");
            
            // 2. 각종 옵션들 추가
            foreach (var option in _options) sb.Append(option).Append(" ");
            
            // 3. 출력 경로 추가
            sb.Append(_output);

            return sb.ToString().Trim();
        }
    }
}
