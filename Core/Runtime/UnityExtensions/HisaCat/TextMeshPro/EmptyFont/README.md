# Empty Font

이는 I18N Font등을 위한 비어있는 Font Asset입니다.

오픈소스인 [Adobe Blank](https://github.com/adobe-fonts/adobe-blank) 를 사용하여 제작되었습니다.

## 1. 제작 과정

1. `AdobeBlank.ttf` 우클릭 뒤 `Create/TextMeshPro/Font Asset/SDF` 선택
2. 생성된 `AdobeBlank SDF` 를 선택
3. Atlas Population Mode를 `Static`으로 변경
4. 이후 `Update Atlas Texture` 버튼을 클릭하여 `Font Asset Creator` 윈도우 열기
5. 다음 설정 적용:
   > Sampling Point Size: `Auto Sizing` (기본값)  
   > Padding: `0px`  
   > Packing Method: `Fast` (기본값)  
   > Atlas Resolution: `8x8`  
   > Character Set: `Custom Range`  
   > Character Sequence (Decimal): 빈칸  
   > Render Mode: `SDFAA` (기본값)  
   > Get Font Features: `False` (기본값)
7. `Generate Font Atlas` 버튼 클릭
8. `Save` 버튼 클릭
9. 폰트 어셋 `AdobeBlank SDF` 설정 창에서 `Source Font File`을 제거  
   > ❔ 이는 선택사항으로, `AdobeBlank.ttf` 폰트와의 참조를 끊어,  
   > 원본 폰트 파일이 빌드에 포함되는 것을 방지하기 위한 조치입니다.  
   
   > ⚠ 이 과정에서 간혈적으로 `Face Info` 가 변경되는 발생할 수 있음으로 주의하세요.  
   > 임시로 `TextMeshPro` 컴포넌트에 이 폰트를 할당하거나, 어셋 자체를 복제하는 등의 작업을 거치면 이 문제가 해결됩니다.  
   
   > ✅ 사전에 생성된 `EmptyFont`는 이 과정이 포함되어 있습니다.
10. 폰트 어셋 이름 변경: `AdobeBlank SDF` -> `EmptyFont`
