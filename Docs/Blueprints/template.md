# [영상 편집 및 포맷 변환 - MVVM 전면 개편] Blueprint

**작성일**: 2026-04-17
**작성자**: Antigravity (AI Assistant)
**목표**: 앱을 MVVM 아키텍처로 리팩토링하고, 이미지/동영상 폴더 보기 지원 및 동영상 포맷 변환 기능을 추가합니다.

---

## 1. 현재 상태 (Context)
*   **구조**: 현재 단일 윈도우(`MainWindow.xaml.cs`)에 모든 비즈니스 로직과 UI 업데이트 로직이 포함되어 있어 확장이 어렵습니다.
*   **기능**: 이미지 파일만 필터링하여 보여주고 있으며, 미리보기도 이미지 전용입니다.
*   **요구 사항**: 
    1.  전체 구조를 **MVVM (Model-View-ViewModel)**으로 전환하여 유지보수성을 확보합니다.
    2.  동영상 파일 인식을 추가하고 미리보기를 제공합니다.
    3.  동영상을 다른 포맷으로 **변환(Converter)**하는 편집 기능을 구현합니다.

## 2. 세부 설계 및 계획 (Design & Plan)
*   **MVVM 전환**: 
    - `Base`: `ObservableObject`, `RelayCommand` 구현.
    - `ViewModel`: `MainViewModel`에서 파일 목록, 로그, 명령(Open, Convert 등) 관리.
    - `View`: `MainWindow`는 `DataContext` 바인딩을 통해 데이터 및 명령과 연결.
*   **편집 및 변환 로직**: 
    - `MediaService`: `ffmpeg`를 활용하여 추출 및 변환 명령어 실행.
    - `UI`: 변환할 타겟 포맷을 선택할 수 있는 `ComboBox` 추가.

## 3. 진행 상황 (Progress)
*   [ ] MVVM 기본 클래스(`RelayCommand`, `ObservableObject`) 생성
*   [ ] `MainViewModel` 및 서비스 레이어(`FileService`, `MediaService`) 구축
*   [ ] `MainWindow.xaml` 데이터 바인딩 및 UI 컨트롤 전환
*   [ ] 영상/이미지 통합 필터링 및 미리보기 로직 구현
*   [ ] FFmpeg 연동 포맷 변환 기능 구현 (MP4, MKV 등)

## 4. 논의 사항 및 미결 문제 (Open Questions)
*   **FFmpeg 관리**: 사용자가 FFmpeg 경로를 설정할 수 있도록 할지, 전용 경로를 강제할지 결정이 필요합니다.
*   **MVVM 라이브러리**: 외부 라이브러리(CommunityToolkit 등) 사용 여부 (현재는 코드 복잡도를 낮추기 위해 직접 구현 예정).

## 5. 향후 일정 (Next Steps)
*   완전한 MVVM 전환 후, 복잡한 편집 기능(구간 자르기 등) 추가.
*   변환 진행 상항을 프로그레스 바와 로그를 통해 시각화.
