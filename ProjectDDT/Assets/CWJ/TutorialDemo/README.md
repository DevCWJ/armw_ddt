# Tutorial UI Highlight System - 첫 세팅 가이드

> ✅ **TutorialDemoScene**은 초기 세팅이 완료된 상태입니다.  
> 새로운 씬에서 처음 설정 시 아래 순서를 따르세요.  
> 순서에 따랐을때 동작하지 않을시 **TutorialDemoScene** 씬부터 참고후 문의해주세요.

---

## 🚀 초기 세팅 방법

1. **씬에 `UIFeedbackInjector` 컴포넌트를 포함한 GameObject를 생성합니다.**
   💡 참고: `UIFeedbackInjector`는 각종 상호작용가능한 UI컴포넌트 (버튼, 토글, 등등)에 상호작용시   
   tween효과와 사운드가 실행되도록 해주는 헬퍼클래스입니다   


2. **씬에 `UIHighlightManager` 컴포넌트를 포함한 GameObject를 생성합니다.**  
   ⚠️ `UIHighlightManager`, `UIFeedbackInjector`는 **싱글톤**이므로 **씬에 하나만 존재**해야 합니다.


3. **씬에 `TutorialDemoTool` 컴포넌트를 포함한 GameObject를 생성하고,**  
   인스펙터에서 `tutorialLayers` 변수(튜토리얼 가이드라인 순서 및 설정)를 지정합니다.  
   💡 참고: `TutorialDemoScene`에서는 `"T - 9Page"`라는 이름의 오브젝트로 존재합니다.  
   ⚠️ `TutorialDemoTool`은 **챕터별 여러 개** 만들어도 됩니다.


4. **캔버스 내 이미지 또는 `RectTransform`이 있는 오브젝트에**  
   `UIHighlightHandler` 컴포넌트를 추가하면 해당 오브젝트에 **hole-fade 효과**가 적용됩니다.  

---

## ⚙️ 추가 설정

### ✅ 페이드 관련 전체 설정
- **전체 효과**: `UIHighlightManager`의 인스펙터에서 설정  
- **개별 tween 효과**: 각 `UIHighlightHandler`의 인스펙터에서 설정

### ✅ 전체 기능 활성화/비활성화
- `UIHighlightManager.enabled`를 통해 제어

### ✅ 최대 hole 갯수 수정 방법 (기본값: 7)

1. `UIHoleFadeImage` 스크립트 내에서 다음 줄 수정:
   ```csharp
   const int _MaxHolesCount = 7; // 원하는 숫자로 변경
   ```
2. `UIHoleFade.shader` 파일의 **94번째 줄**에서 다음 줄 수정:
   ```hlsl
   uniform float4 _Holes[7]; // 원하는 숫자로 변경
   ```
3. **Unity 에디터를 재시작**하세요.  
   → Unity의 최적화 특성상 해당 설정은 동적으로 변경되지 않으며, **최대치를 고정**해야 합니다.

---

## 📩 추가 문의

- 담당자: **조우정**  
- 이메일: [cwj@kakao.com](mailto:cwj@kakao.com)
