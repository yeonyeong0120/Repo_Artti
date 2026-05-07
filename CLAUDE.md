# 아르띠 (Artti) — 발달장애인 지역사회 소통 지원 AI·AAC 통합 플랫폼

## 프로젝트 개요

캡스톤 디자인 프로젝트. 발달장애인의 지역사회 소통 참여를 지원하는 듀얼 모드 Unity 앱.

- 훈련모드: 약국·편의점·음식점 시나리오에서 LLM(Gemini Flash) 대화 매니저 기반 발화 훈련
- AR현장모드: 카메라로 간판 인식 후 OCR 기반 AAC 카드 표시 (오프라인 동작)

상세 기획은 docs/PLAN.md 참조 (개발기획서 v3 전문). 작업 지시 시 기획서 절 번호로 참조 가능 (예: "기획서 5.5절 기반").

## 환경

- Unity 2022.3.62f3
- 타겟: Android 11 이상
- 비동기: UniTask (Cysharp.Threading.Tasks)
- LLM: Google Gemini 2.5 Flash (function calling, streaming)
- OCR: Google ML Kit (온디바이스)
- AR: AR Foundation

## 폴더 컨벤션

- 모든 자체 코드/자산: Assets/_Project/ 아래
- 스크립트: Assets/_Project/_Scripts/ 아래 기능별 분류 (AAC, Training, ARField, Common 등)
- 데이터: Assets/_Project/_Data/ 아래 (AAC 카드 ScriptableObject, 시나리오 JSON 등)
- 씬: Assets/_Project/_Scenes/

## 코딩 규칙 (반드시 지킬 것)

### 아키텍처
- MonoBehaviour는 view 갱신과 입력 처리만 담당. 비즈니스 로직, 데이터 모델, 외부 API 연동은 분리.
- Update() 안에서 GetComponent, FindObjectOfType, LINQ 체인, 문자열 연결 금지. 참조는 Awake/Start에서 캐싱.

### 비동기
- 표준 Task 사용 금지. UniTask 사용.
- 모든 async 작업은 CancellationToken을 받고 명시적으로 취소 처리.
- Coroutine은 렌더 루프와 강결합된 경우만 (WaitForEndOfFrame 등).

### 메모리/성능
- JSON: 기본 JsonUtility 또는 System.Text.Json. 큰 페이로드는 Utf8Json/MemoryPack 검토.
- 자주 생성/소멸되는 오브젝트는 ObjectPool 사용.
- 성능 주장은 측정 지표 기반 (Profiler ms, allocation KB, frame time).

## 코드 작성 규칙

- 부분 코드만 출력. 변경 지점은 주석으로 명시.
- 전체 파일은 사용자가 명시적으로 요청하거나, 새 클래스 도입, using/namespace 변경 시에만.
- 코드, 인라인 주석, 코드 설명에 이모지 사용 금지.

## 작업 흐름

- 작업 시작 전 사용자가 git commit으로 깨끗한 상태 확보.
- 큰 변경은 작업 단위로 쪼개기. "전체 시나리오 시스템" 같은 명령보다 "약국 시나리오의 greeting objective 처리"처럼 작게.
- Unity 자동 컴파일 충돌 방지: Auto Refresh와 Reload Domain은 사용자가 미리 끔. mcp-unity 도구 호출 직전 사용자가 수동 Refresh.

## 팀

- 팀장: 김연영 (훈련모드·대화 매니저 담당)
- 팀원: 방승훈 (UI·AAC DB·시각 리소스 담당)
- 팀원: 오정훈 (AR현장모드 담당)
