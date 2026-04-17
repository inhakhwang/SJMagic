using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using WinForms = System.Windows.Forms;

namespace SJMagic
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileInfo> FileList { get; set; } = new ObservableCollection<FileInfo>();

        public MainWindow()
        {
            InitializeComponent();
            lbFiles.ItemsSource = FileList;
            
            // 초기 환영 메시지 로깅
            Log("SJMagic 애플리케이션이 시작되었습니다.", "INFO");
            Log("시스템 상태를 점검하는 중...", "DEBUG");
            Log("메인 레이아웃 초기화 완료.", "SUCCESS");
        }

        /// <summary>
        /// 하단 로그 창에 메시지를 출력합니다.
        /// </summary>
        public void Log(string message, string level = "INFO")
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] [{level}] {message}\n";
            
            txtLog.AppendText(logEntry);
            txtLog.ScrollToEnd();
            
            // 상태 표시줄 업데이트
            tbLogStatus.Text = $" - Last Activity: {timestamp}";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                Log($"Menu Item Clicked: {item.Header}", "EVENT");
                
                if (item.Header.ToString() == "Open Folder")
                {
                    OpenFolderBrowser();
                }
            }
        }

        private void OpenFolderBrowser()
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "이미지가 있는 폴더를 선택하세요";
                dialog.ShowNewFolderButton = false;

                // WinForms Dialog 결과 확인 (WPF에서 사용할 때는 WindowInteropHelper가 좋지만 기본으로도 동작함)
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;
                    Log($"선택된 폴더: {selectedPath}", "PROCESS");
                    LoadImagesFromFolder(selectedPath);
                }
                else
                {
                    Log("폴더 선택 취소됨.", "INFO");
                }
            }
        }

        private void LoadImagesFromFolder(string folderPath)
        {
            try
            {
                FileList.Clear();
                var dirInfo = new DirectoryInfo(folderPath);
                
                // 지원할 확장자 목록
                string[] extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                
                var files = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                   .Where(f => extensions.Contains(f.Extension.ToLower()));

                int count = 0;
                foreach (var file in files)
                {
                    FileList.Add(file);
                    count++;
                }

                Log($"총 {count}개의 이미지 파일을 찾았습니다.", "SUCCESS");
            }
            catch (Exception ex)
            {
                Log($"폴더 읽기 오류: {ex.Message}", "ERROR");
            }
        }

        private void lbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbFiles.SelectedItem is FileInfo selectedFile)
            {
                try
                {
                    Log($"이미지 뷰어: {selectedFile.Name} 로딩 중...", "PROCESS");
                    
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 파일을 잠그지 않고 메모리로 바로 로드
                    bitmap.UriSource = new Uri(selectedFile.FullName);
                    bitmap.EndInit();
                    
                    imgDisplay.Source = bitmap;
                    Log($"이미지 로딩 완료: {selectedFile.Name} ({bitmap.PixelWidth}x{bitmap.PixelHeight})", "SUCCESS");
                }
                catch (Exception ex)
                {
                    Log($"이미지 로딩 실패: {ex.Message}", "ERROR");
                }
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            Log("SJMagic 정보 창 열기", "EVENT");
            string infoMessage = "✨ SJMagic 워크스페이스 ✨\n\n" +
                                 "강력한 레이저 및 이미지 처리 스튜디오입니다.\n" +
                                 "• 버전: 1.0.0 (BETA)\n" +
                                 "• 테마: Deep Sky Blue 딕셔너리 기반 다크 테마\n" +
                                 "• 오빠의 든든한 파트너 수지가 함께합니다! 💖";
            
            MessageBox.Show(infoMessage, "About SJMagic", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
