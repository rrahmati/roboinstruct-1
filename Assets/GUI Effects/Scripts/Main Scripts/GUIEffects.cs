using UnityEngine ;
using System.Collections ;
using UnityEngine.UI ;

public class GUIEffects : MonoBehaviour {

	// common params
	[HideInInspector] public 		float 			commonSpeed 						= 1.0f ;
	[HideInInspector] public 		float 			commonStartDelay 				= 0f ;
	[HideInInspector] public 		float 			commonDismissDelay 			= 0f ;
	[HideInInspector] public 		bool 			commonBounceBack 			= false ;

	// fade variable
	[HideInInspector] public 		float 			fadeSpeed 							= 1.0f ;
	[HideInInspector] public 		float 			fadeDelay 							= 0f ;
	[HideInInspector] public 		float 			fadeDismissDelay 				= 0f ;
	[HideInInspector] public 		float 			fadeStartValue 						= 0f ;
	[HideInInspector] public 		float 			fadeEndValue 						= 1.0f ;

	// move from variables
	[HideInInspector] public 		float 			positionEffectSpeed 				= 1.0f ;
	[HideInInspector] public 		float 			positionEffectDelay 				= 0f ;
	[HideInInspector] public 		float 			positionEffectDismissDelay 	= 0f ;
	[HideInInspector] public		bool			isBounceBackPosition 			= true ;
	
	// scale variables
	[HideInInspector] public 		float 			scaleEffectSpeed 					= 1.0f ;
	[HideInInspector] public 		float 			scaleEffectDelay		 			= 0f ;
	[HideInInspector] public 		float 			scaleEffectDismissDelay 		= 0f ;
	[HideInInspector] public 		float 			popUpStartValue 					= 0f ;
	[HideInInspector] public 		float 			reversePopUpStartValue 		= 5.0f ;
	[HideInInspector] public 		bool			isBounceBackScaleEffect 		= true ;

	// rotation variables
	[HideInInspector] public 		float 			rotationSpeed 						= 1.0f ;
	[HideInInspector] public 		float 			rotationDelay 						= 0f ;
	[HideInInspector] public 		float 			rotationDismissDelay 			= 0f ;
	[HideInInspector] public 		float 			rotationDismissTime 			= 1.0f ;
	[HideInInspector] public 		bool 			isContinuosRotation 			= false ;
	[HideInInspector] public  		bool 			axisX 									= false ;
	[HideInInspector] public 		bool 			axisY 									= false ;
	[HideInInspector] public 		bool 			axisZ 									= false ;
	[HideInInspector] public 		bool 			isBounceBackRotation 			= true ;
	[HideInInspector] public 		int 			numberOfRotations 				= 1 ;
	[HideInInspector] public 		int 			flipsPerRotation 					= 1 ;
	[HideInInspector] public 		float 			delayBetweenTwoRotation 	= 0.01f;

	// all booleans
	[HideInInspector] public	 	bool 			ignoreTimeScale 					= false ;
	[HideInInspector] public 		bool 			isClickable 							= false ;
	[HideInInspector] public 		bool 			isDestroyOnDismiss 				= false ;
	[HideInInspector] public 		bool 			isCommonParams 				= false ;
	[HideInInspector] public 		bool 			isPanel 									= false ;
	[HideInInspector] public 		bool 			isRandom 							= false ;
	[HideInInspector] public 		bool 			isColorChangeFoldOpen 		= false ;
	[HideInInspector] public 		bool 			isFade 									= false ;
	[HideInInspector] public 		bool 			isPositionChangeFoldOpen 	= false ;
	[HideInInspector] public 		bool 			isMoveFromBottom 				= false ;
	[HideInInspector] public 		bool 			isMoveFromLeft 					= false ;
	[HideInInspector] public 		bool 			isMoveFromRight 					= false ;
	[HideInInspector] public 		bool 			isMoveFromTop 					= false ;
	[HideInInspector] public 		bool 			isMoveFromTopRight 			= false ;
	[HideInInspector] public 		bool 			isMoveFromTopLeft 				= false ;
	[HideInInspector] public 		bool 			isMoveFromBottomRight 		= false ;
	[HideInInspector] public 		bool 			isMoveFromBottomLeft 		= false ;
	[HideInInspector] public 		bool 			isScaleChangeFoldOpen 		= false ;
	[HideInInspector] public 		bool 			isPopUp 								= false ;
	[HideInInspector] public 		bool 			isReversePopUp 					= false ;
	[HideInInspector] public 		bool 			isRotationChangeFoldOpen 	= false ;
	[HideInInspector] public 		bool 			isRotation 							= false ;

	// dismiss objects on click
	public 						GameObject[] 		dismissOnClick ;

	// booleans
	private 		bool 			isButton 								= false ;
	private 		bool 			isText 									= false ;
	private 		bool 			isImage 								= false ;
	private 		bool 			isRawImage 							= false ;
	private 		bool 			isSlider 								= false ;
	private 		bool 			isScrollBar 							= false ;
	private 		bool 			isToggle 								= false ;
	private 		bool 			isInputField 							= false ;


	private 		float 			tempFadeAlpha ;
	private 		float 			tempFadeAlphaAddition ;
	private 		float 			tempFadeFrames 					= 0 ;
	private 		bool 			fadeCompleted 					= false ;
	private 		int 			tempNumberOfRotations 		= 0 ;
	private 		Vector3 	rotationVector ;

	// Use this for initialization
	void Start ( ) {
		tempFadeAlpha 						= fadeStartValue ;
		tempFadeAlphaAddition 			= ( ( fadeEndValue - fadeStartValue ) / ( (2.0f/fadeSpeed) * 50.0f ) ) ;

		WhichObjectIsThis();
		// check if common parameters
		if( isCommonParams ){
			fadeSpeed 					= positionEffectSpeed 				= scaleEffectSpeed				= rotationSpeed 				= commonSpeed ;
			fadeDelay 					= positionEffectDelay 				= scaleEffectDelay				= rotationDelay 				= commonStartDelay ;
			fadeDismissDelay 		= positionEffectDismissDelay 	= scaleEffectDismissDelay 	= rotationDismissDelay 	= commonDismissDelay ;
			isBounceBackPosition 	= isBounceBackScaleEffect 		= isBounceBackRotation 		= commonBounceBack ;
		}

		// check if random
		if( isRandom ){
			ApplyRandomEffects();
		}

		if( isMoveFromBottom ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromLeft ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromRight ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromTop ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromTopRight ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromTopLeft ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromBottomRight ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromBottomLeft ){
			if( isBounceBackPosition ){
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveFrom( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isPopUp ){
			if( isBounceBackScaleEffect ){
				GUITween.ScaleFrom( gameObject , GUITween.Hash("scale", new Vector3( popUpStartValue, popUpStartValue, popUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.ScaleFrom( gameObject , GUITween.Hash("scale", new Vector3( popUpStartValue, popUpStartValue, popUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isReversePopUp ){
			if( isBounceBackScaleEffect ){
				GUITween.ScaleFrom( gameObject , GUITween.Hash("scale", new Vector3( reversePopUpStartValue, reversePopUpStartValue, reversePopUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.ScaleFrom( gameObject , GUITween.Hash("scale", new Vector3( reversePopUpStartValue, reversePopUpStartValue, reversePopUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isRotation ){
			if( axisX ){
				rotationVector = new Vector3( flipsPerRotation, 0, 0 );
			}else
			if( axisY ){
				rotationVector = new Vector3( 0, flipsPerRotation, 0);
			}else
			if( axisZ ){
				rotationVector = new Vector3( 0, 0, flipsPerRotation );
			}else{
				rotationVector = new Vector3( flipsPerRotation, flipsPerRotation, flipsPerRotation );
				Debug.LogError("You have not selected your axis to rotate in GUIEffects. Please select any one axis in the inspector on your GUI element.");
			}
			if( isContinuosRotation ){
				if( isBounceBackRotation ){
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateContinuos" ));
				}else{
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.linear, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateContinuos" ));
					}
			}else{
				if( isBounceBackRotation ){
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateNonContinuos" ));
				}else{
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.linear, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateNonContinuos" ));
				}
			}
		}
	}
	// continue rotations
	void WaitAndRotateContinuos(){
		if( isBounceBackRotation ){
			GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", delayBetweenTwoRotation, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateContinuos" ));
		}else{
			GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", delayBetweenTwoRotation, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.linear, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateContinuos" ));
		}
	}
	// limited rotations
	void WaitAndRotateNonContinuos(){
		tempNumberOfRotations++;
		if( tempNumberOfRotations < (numberOfRotations) ){
			if( isBounceBackRotation ){
				GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", delayBetweenTwoRotation, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateNonContinuos" ));
			}else{
				GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", delayBetweenTwoRotation, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.linear, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateNonContinuos" ));
			}
			Debug.Log(tempNumberOfRotations);
		}
	}



	// fixed update
	void FixedUpdate(){
		tempFadeFrames++;
		// fading effect
		if( isFade && tempFadeFrames >= fadeDelay * 50 ){
			if( isButton ){
				gameObject.GetComponent<Image>().color = new Color( gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform childText = transform.GetChild( 0 );
				childText.gameObject.GetComponent<Text>().color = new Color( childText.gameObject.GetComponent<Text>().color.r, childText.gameObject.GetComponent<Text>().color.g, childText.gameObject.GetComponent<Text>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isText ){
				gameObject.GetComponent<Text>().color = new Color( gameObject.GetComponent<Text>().color.r, gameObject.GetComponent<Text>().color.g, gameObject.GetComponent<Text>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isImage ){
				gameObject.GetComponent<Image>().color = new Color( gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isRawImage ){
				gameObject.GetComponent<RawImage>().color = new Color( gameObject.GetComponent<RawImage>().color.r, gameObject.GetComponent<RawImage>().color.g, gameObject.GetComponent<RawImage>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isSlider ){
				Transform bg = transform.GetChild( 0 );
				bg.GetComponent<Image>().color = new Color( bg.GetComponent<Image>().color.r, bg.GetComponent<Image>().color.g, bg.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform fill = transform.GetChild( 1 ).transform.GetChild( 0 );
				fill.GetComponent<Image>().color = new Color( fill.GetComponent<Image>().color.r, fill.GetComponent<Image>().color.g, fill.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform handle = transform.GetChild( 2 ).transform.GetChild( 0 );
				handle.GetComponent<Image>().color = new Color( handle.GetComponent<Image>().color.r, handle.GetComponent<Image>().color.g, handle.GetComponent<Image>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isScrollBar ){
				gameObject.GetComponent<Image>().color = new Color( gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform hand = transform.GetChild(0).transform.GetChild(0);
				hand.GetComponent<Image>().color = new Color( hand.GetComponent<Image>().color.r, hand.GetComponent<Image>().color.g, hand.GetComponent<Image>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isToggle ){
				Transform bgToggle = transform.GetChild(0);
				bgToggle.GetComponent<Image>().color = new Color( bgToggle.GetComponent<Image>().color.r, bgToggle.GetComponent<Image>().color.g, bgToggle.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform checkmark = transform.GetChild(0).transform.GetChild(0);
				checkmark.GetComponent<Image>().color = new Color( checkmark.GetComponent<Image>().color.r, checkmark.GetComponent<Image>().color.g, checkmark.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform label = transform.GetChild(1);
				label.GetComponent<Text>().color = new Color( label.GetComponent<Text>().color.r, label.GetComponent<Text>().color.g, label.GetComponent<Text>().color.b, tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
			if( isInputField ){
				gameObject.GetComponent<Image>().color = new Color( gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, tempFadeAlpha );
				Transform placeHold = transform.GetChild( transform.childCount - 2 );
				placeHold.GetComponent<Text>().color = new Color( placeHold.GetComponent<Text>().color.r, placeHold.GetComponent<Text>().color.g, placeHold.GetComponent<Text>().color.b , tempFadeAlpha );
				Transform inputText = transform.GetChild( transform.childCount -1 );
				inputText.GetComponent<Text>().color = new Color( inputText.GetComponent<Text>().color.r, inputText.GetComponent<Text>().color.g, inputText.GetComponent<Text>().color.b , tempFadeAlpha );
				if( !fadeCompleted && tempFadeAlpha <= fadeEndValue ){
					tempFadeAlpha += tempFadeAlphaAddition ;
				}
				if( fadeCompleted && tempFadeAlpha >= fadeStartValue ){
					tempFadeAlpha -= tempFadeAlphaAddition ;
				}
			}
		}

	}

	// check which object is this
	void WhichObjectIsThis(){
		if( gameObject.GetComponent<Button>() == true && gameObject.GetComponent<Image>() == true ){
			isButton = true;
		}
		if( gameObject.GetComponent<Text>() == true ){
			isText = true;
		}
		if( gameObject.GetComponent<Image>() == true && gameObject.GetComponent<Button>() == false && gameObject.GetComponent<Scrollbar>() == false && gameObject.GetComponent<InputField>() == false && isPanel == false ){
			isImage = true;
		}
		if( gameObject.GetComponent<RawImage>() == true ){
			isRawImage = true;
		}
		if( gameObject.GetComponent<Slider>() == true ){
			isSlider = true;
		}
		if( gameObject.GetComponent<Scrollbar>() == true && gameObject.GetComponent<Image>() == true ){
			isScrollBar = true;
		}
		if( gameObject.GetComponent<Toggle>() == true ){
			isToggle = true;
		}
		if( gameObject.GetComponent<InputField>() == true && gameObject.GetComponent<Image>() == true ){
			isInputField = true;
		}
		return;
	}

	// random
	public void ApplyRandomEffects(){
		isFade = isMoveFromBottom = isMoveFromLeft = isMoveFromRight = isMoveFromTop = isMoveFromBottomLeft = isMoveFromBottomRight = isMoveFromTopLeft = isMoveFromTopRight = isPopUp = isReversePopUp = isRotation = false ;
		int totalRandomEffects = Random.Range( 1, 5 );
		switch( totalRandomEffects ){
		case 1:
			int oneRandom = Random.Range( 1,5 );
			ApplyMe( oneRandom );
			break;

		case 2:
												int twoRandomFirst = Random.Range( 1,5 );
			TWO_RANDOM_TRY : 	int twoRandomSecond = Random.Range( 1,5 );
			if( twoRandomSecond == twoRandomFirst ){
				goto TWO_RANDOM_TRY;
			}
			ApplyMe( twoRandomFirst );
			ApplyMe( twoRandomSecond );
			break;

		case 3:
															int threeRandomFirst = Random.Range( 1, 5);
			THREE_RANDOM_TRY_ONE : 	int threeRandomSecond = Random.Range( 1, 5);
			if( threeRandomSecond == threeRandomFirst ){
				goto THREE_RANDOM_TRY_ONE ;
			}
			THREE_RANDOM_TRY_TWO : 	int threeRandomThird = Random.Range( 1, 5);
			if( threeRandomThird == threeRandomFirst || threeRandomThird == threeRandomSecond ){
				goto THREE_RANDOM_TRY_TWO ;
			}
			ApplyMe( threeRandomFirst );
			ApplyMe( threeRandomSecond );
			ApplyMe( threeRandomThird );
			break;

		case 4:
			ApplyMe( 1 );
			ApplyMe( 2 );
			ApplyMe( 3 );
			ApplyMe( 4 );
			break;
		}
	}
	void ApplyMe( int temp ){
		switch( temp ){
		case 1:
			ApplyFadeForRandom();
			break;
		case 2:
			ApplyMoveForRandom();
			break;
		case 3:
			ApplyScaleForRandom();
			break;
		case 4:
			ApplyRotationForRandom();
			break;
		}
	}
	void ApplyFadeForRandom(){
		isFade = true ;
	}
	void ApplyMoveForRandom(){
		int whichMoveEffect = Random.Range( 1, 9 );
		switch(whichMoveEffect){
		case 1:
			isMoveFromBottom = true;
			break;
		case 2:
			isMoveFromLeft = true;
			break;
		case 3:
			isMoveFromRight = true;
			break;
		case 4:
			isMoveFromTop = true;
			break;
		case 5:
			isMoveFromBottomLeft = true;
			break;
		case 6:
			isMoveFromBottomRight = true;
			break;
		case 7:
			isMoveFromTopLeft = true;
			break;
		case 8:
			isMoveFromTopRight = true;
			break;
		}
	}
	void ApplyScaleForRandom(){
		int whichScaleEffect = Random.Range( 1, 3 );
		switch( whichScaleEffect ){
		case 1:
			isPopUp = true;
			break;
		case 2:
			isReversePopUp = true;
			break;
		}
	}
	void ApplyRotationForRandom(){
		int whichRotationEffect = Random.Range( 1, 4 );
		isRotation = true ;
		switch( whichRotationEffect ){
		case 1:
			axisX = true;
			break;
		case 2:
			axisY = true;
			break;
		case 3:
			axisZ = true;
			break;
		}
	}

	// dismiss global function
	public void DismissObjects(){
		if( dismissOnClick.Length > 0 ){
			for( int i = 0 ; i < dismissOnClick.Length ; i++ ){
				dismissOnClick[i].GetComponent<GUIEffects>().DismissNow();
			}
		}
	}

	// Dismiss local
	public void DismissNow(){
		if( isFade ){
			StartCoroutine( FadeOut() );
			Invoke( "DestroyMe" , fadeDismissDelay + ((2.0f/fadeSpeed) ) );
		}
		if( isMoveFromBottom ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromLeft ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromRight ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromTop ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromTopRight ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromTopLeft ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y + Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromBottomRight ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x + Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isMoveFromBottomLeft ){
			if( isBounceBackPosition ){
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "easeType", GUITween.EaseType.easeInOutBack, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.MoveTo( gameObject , GUITween.Hash("position",new Vector3( gameObject.GetComponent<RectTransform>().localPosition.x - Screen.width , gameObject.GetComponent<RectTransform>().localPosition.y - Screen.height , gameObject.GetComponent<RectTransform>().localPosition.z ), "islocal", true, "time", 2.0f/positionEffectSpeed , "delay", positionEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isPopUp ){
			if( isBounceBackScaleEffect ){
				GUITween.ScaleTo( gameObject , GUITween.Hash("scale", new Vector3( popUpStartValue, popUpStartValue, popUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDismissDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.ScaleTo( gameObject , GUITween.Hash("scale", new Vector3( popUpStartValue, popUpStartValue, popUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isReversePopUp ){
			if( isBounceBackScaleEffect ){
				GUITween.ScaleTo( gameObject , GUITween.Hash("scale", new Vector3( reversePopUpStartValue, reversePopUpStartValue, reversePopUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDismissDelay, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale ));
			}else{
				GUITween.ScaleTo( gameObject , GUITween.Hash("scale", new Vector3( reversePopUpStartValue, reversePopUpStartValue, reversePopUpStartValue ), "islocal", true, "time", 2.0f/scaleEffectSpeed, "delay", scaleEffectDismissDelay, "ignoretimescale" , ignoreTimeScale ));
			}
		}
		if( isRotation ){
			if( axisX ){
				rotationVector = new Vector3( -1.0f * flipsPerRotation, 0, 0 );
			}else
			if( axisY ){
				rotationVector = new Vector3( 0, -1.0f * flipsPerRotation, 0);
			}else
			if( axisZ ){
				rotationVector = new Vector3( 0, 0, -1.0f * flipsPerRotation );
			}else{
				rotationVector = new Vector3( flipsPerRotation, flipsPerRotation, flipsPerRotation );
				Debug.LogError("You have not selected your axis to rotate in GUIEffects. Please select any one axis in the inspector on your GUI element.");
			}
			if( isContinuosRotation ){
				if( isBounceBackRotation ){
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDismissDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateContinuos" ));
				}else{
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDismissDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.linear, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateContinuos" ));
				}
			}else{
				if( isBounceBackRotation ){
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDismissDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.spring, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateNonContinuos" ));
				}else{
					GUITween.RotateBy( gameObject, GUITween.Hash("amount", rotationVector, "time", 2.0f/rotationSpeed, "delay", rotationDismissDelay, "looptype", GUITween.LoopType.none, "easeType", GUITween.EaseType.linear, "ignoretimescale" , ignoreTimeScale, "oncomplete", "WaitAndRotateNonContinuos" ));
				}
			}
			Invoke( "DestroyMe" , rotationDismissDelay + rotationDismissTime );
		}



		// destroy
		if( isMoveFromBottom || isMoveFromLeft || isMoveFromRight || isMoveFromTop || isMoveFromBottomLeft || isMoveFromBottomRight || isMoveFromTopLeft || isMoveFromTopRight ){
			Invoke( "DestroyMe" , positionEffectDismissDelay + 2.0f/positionEffectSpeed );
		}
		if( isPopUp || isReversePopUp ){
			Invoke( "DestroyMe" , scaleEffectDismissDelay + 2.0f/scaleEffectSpeed );
		}
	}

	// destroy any one
	public void DestroyMe(){
		if( isDestroyOnDismiss ){
			Destroy( gameObject );
		}
	}

	// wait for fade out
	IEnumerator FadeOut(){
		yield return new WaitForSeconds( fadeDismissDelay );
		fadeCompleted = true ;
	}
}
