using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SJMagic.Core.Media
{
    /// <summary>
    /// MP4 파일의 내부 구조(Atom/Box)를 순수 C# 코드로 분석하는 교육용 클래스입니다.
    /// 외부 라이브러리 없이 바이트 단위로 파일을 읽어 구조를 파싱합니다.
    /// </summary>
    public class Mp4StructureParser
    {
        private readonly List<string> _structureLog = new List<string>();

        /// <summary>
        /// MP4 파일의 뼈대(Box)를 분석하여 문자열 리스트로 반환합니다.
        /// </summary>
        public List<string> ParseStructure(string filePath)
        {
            _structureLog.Clear();
            if (!File.Exists(filePath)) return _structureLog;

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    _structureLog.Add($"[분석 시작] {Path.GetFileName(filePath)}");
                    ParseBoxes(fs, 0, fs.Length, 0);
                }
            }
            catch (Exception ex)
            {
                _structureLog.Add($"[오류] 분석 중 예외 발생: {ex.Message}");
            }

            return _structureLog;
        }

        /// <summary>
        /// 지정된 범위 내의 모든 Box들을 재귀적으로 찾아냅니다.
        /// [학습 포인트]
        /// - MP4 파일은 하위 상자를 가질 수 있는 계층 구조(Tree)입니다.
        /// - 각 상자는 [4바이트 크기] + [4바이트 이름]으로 시작합니다.
        /// </summary>
        private void ParseBoxes(FileStream fs, long startOffset, long length, int depth)
        {
            long currentOffset = startOffset;
            string indent = new string(' ', depth * 4);

            while (currentOffset + 8 <= startOffset + length)
            {
                fs.Seek(currentOffset, SeekOrigin.Begin);

                // 1. 크기 읽기 (4바이트, Big Endian)
                byte[] sizeBuffer = new byte[4];
                fs.Read(sizeBuffer, 0, 4);
                uint size = ReadUint32(sizeBuffer);

                // 2. 타입 이름 읽기 (4바이트, ASCII)
                byte[] typeBuffer = new byte[4];
                fs.Read(typeBuffer, 0, 4);
                string type = Encoding.ASCII.GetString(typeBuffer);

                // 로그 기록
                _structureLog.Add($"{indent}▶ [{type}] 위치: {currentOffset}, 크기: {size}");

                // [학습 포인트] moov, trak, mdia 등은 내부에 다른 Box들을 포함하는 '컨테이너' 박스입니다.
                if (IsContainerBox(type))
                {
                    // 내부를 더 깊게 파고듭니다 (재귀 호출)
                    ParseBoxes(fs, currentOffset + 8, size - 8, depth + 1);
                }

                // 다음 박스로 이동
                if (size == 0) break; // 파일 끝까지라는 의미
                currentOffset += size;
            }
        }

        /// <summary>
        /// 다른 박스를 포함할 수 있는 '부모 박스'들인지 확인합니다.
        /// </summary>
        private bool IsContainerBox(string type)
        {
            switch (type)
            {
                case "moov": // 영화 메타데이터 전체
                case "trak": // 개별 트랙 (영상, 오디오 등)
                case "mdia": // 미디어 정보
                case "minf": // 미디어 정보 헤더
                case "stbl": // 샘플 테이블 (실제 데이터 위치 정보가 담긴 곳)
                case "udta": // 사용자 데이터
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 4바이트 버퍼를 Big Endian 방식의 32비트 정수로 변환합니다.
        /// [학습 포인트] 네트워크와 미디어 파일 포맷은 보통 높은 자리수가 앞에 오는 Big Endian을 사용합니다.
        /// </summary>
        private uint ReadUint32(byte[] buffer)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToUInt32(buffer, 0);
        }
    }
}
