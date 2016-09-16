using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof( GUIEffects ))]

public class GUIEffectsEditor : Editor
{
	private 	GUIEffects 		effectScript;

	private int spaceValue = 10 ;

	// when enabled
	void OnEnable()
	{
		effectScript = ( GUIEffects )target ;
	}


	// gui part
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Some GUI effects can not work together. They will be automatically removed, if you use one of them.\nDon't forget to save scene after you make any changes.\nSpeed value of 1, means effect will take 2 seconds to execute.Set your speed according to this calculation.", MessageType.Info , true );

		effectScript.isClickable 										= EditorGUILayout.Toggle( new GUIContent("Clickable", "Is this Clickable item ?"), effectScript.isClickable );
		effectScript.isDestroyOnDismiss 						= EditorGUILayout.Toggle( new GUIContent("Destroy after Dismiss", "Destroy this object after you have dismissed it.") , effectScript.isDestroyOnDismiss );
		effectScript.ignoreTimeScale								= EditorGUILayout.Toggle( new GUIContent("Ignore TimeScale", "Ignore your scene's time scale.") , effectScript.ignoreTimeScale );
		effectScript.isPanel 											= EditorGUILayout.Toggle( new GUIContent("This is Panel", "Is this object a panel ?") , effectScript.isPanel );
		effectScript.isCommonParams 							= EditorGUILayout.Toggle( new GUIContent("Use Common Parameters", "Use common parameters like speed, delay on all effects you choose." ) , effectScript.isCommonParams );
		if( effectScript.isCommonParams ){
			effectScript.commonSpeed 							= EditorGUILayout.FloatField( "\tSpeed (Common)", effectScript.commonSpeed );
			effectScript.commonStartDelay 					= EditorGUILayout.FloatField( "\tDelay on Start (Common)", effectScript.commonStartDelay );
			effectScript.commonDismissDelay 				= EditorGUILayout.FloatField( "\tDelay on Dismiss (Common)", effectScript.commonDismissDelay );
			effectScript.commonBounceBack 					= EditorGUILayout.Toggle( "\tBounce Back (Common)", effectScript.commonBounceBack );
		}
		GUILayout.Space( spaceValue );
		// random
		effectScript.isRandom 										= EditorGUILayout.Toggle( "Random Effect", effectScript.isRandom );
		if( effectScript.isRandom ){
			GUILayout.Space( 2.0f * spaceValue );
		}

		if( !effectScript.isRandom ){
			// fade
			if( !effectScript.isPanel ){
				effectScript.isColorChangeFoldOpen 				= EditorGUILayout.Foldout( effectScript.isColorChangeFoldOpen, "Color Related Effects" );
				if( effectScript.isColorChangeFoldOpen ){
					effectScript.isFade 										= EditorGUILayout.Toggle( "\tFade In", effectScript.isFade );
					if( effectScript.isFade ){
						if( !effectScript.isCommonParams ){
							effectScript.fadeSpeed 								= EditorGUILayout.FloatField( "\t\tSpeed" , effectScript.fadeSpeed );
							effectScript.fadeDelay 								= EditorGUILayout.FloatField( "\t\tDelay on start" , effectScript.fadeDelay);
							effectScript.fadeDismissDelay 					= EditorGUILayout.FloatField( "\t\tDelay on Dismiss", effectScript.fadeDismissDelay );
						}
						effectScript.fadeStartValue 						= EditorGUILayout.Slider( "\t\tFading Starts From" , effectScript.fadeStartValue, 0f, 0.75f );
						effectScript.fadeEndValue 						= EditorGUILayout.Slider( "\t\tFading Ends At" , effectScript.fadeEndValue, effectScript.fadeStartValue + 0.01f, 1.0f );
						EditorGUILayout.HelpBox("Start value should not be greater than end value.", MessageType.Info , false );
						GUILayout.Space( spaceValue );
					}
					GUILayout.Space( spaceValue );
				}
			}

			effectScript.isPositionChangeFoldOpen 			= EditorGUILayout.Foldout( effectScript.isPositionChangeFoldOpen, "Position Related Effects" );
			if( effectScript.isPositionChangeFoldOpen ){
			// move from bottom
				if( !effectScript.isCommonParams ){
					effectScript.positionEffectSpeed 					= EditorGUILayout.FloatField( "\t\tSpeed", effectScript.positionEffectSpeed );
					effectScript.positionEffectDelay 					= EditorGUILayout.FloatField( "\t\tDelay on Start", effectScript.positionEffectDelay );
					effectScript.positionEffectDismissDelay 		= EditorGUILayout.FloatField( "\t\tDelay on Dismiss", effectScript.positionEffectDismissDelay );
					effectScript.isBounceBackPosition 				= EditorGUILayout.Toggle("\t\tBounce Back", effectScript.isBounceBackPosition );
				}

				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromLeft && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomRight && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromBottom 				= EditorGUILayout.Toggle( "\tMove From Bottom", 	effectScript.isMoveFromBottom );
				}

			// move from right
				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromBottom && !effectScript.isMoveFromLeft && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomRight && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromRight 					= EditorGUILayout.Toggle( "\tMove From Right", effectScript.isMoveFromRight );
				}
			
			// move from Left
				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromBottom && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomRight && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromLeft 						= EditorGUILayout.Toggle( "\tMove From Left", 	effectScript.isMoveFromLeft 	 );
				}

			// move from top
				if( !effectScript.isMoveFromBottom && !effectScript.isMoveFromLeft && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomRight && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromTop 						= EditorGUILayout.Toggle( "\tMove From Top", 	effectScript.isMoveFromTop );
				}

				// move from top right corner
				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromBottom && !effectScript.isMoveFromLeft && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomRight && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromTopRight 				= EditorGUILayout.Toggle( "\tMove From Top Right", effectScript.isMoveFromTopRight );
				}

				// move form top left corner
				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromBottom && !effectScript.isMoveFromLeft && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromBottomRight && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromTopLeft 				= EditorGUILayout.Toggle( "\tMove From Top Left", effectScript.isMoveFromTopLeft );
				}

				// move form bottom right corner
				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromBottom && !effectScript.isMoveFromLeft && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomLeft ){
					effectScript.isMoveFromBottomRight 		= EditorGUILayout.Toggle( "\tMove From Bottom Right", effectScript.isMoveFromBottomRight );
				}

				// move form bottom left corner
				if( !effectScript.isMoveFromTop && !effectScript.isMoveFromBottom && !effectScript.isMoveFromLeft && !effectScript.isMoveFromRight && !effectScript.isMoveFromTopRight && !effectScript.isMoveFromTopLeft && !effectScript.isMoveFromBottomRight ){
					effectScript.isMoveFromBottomLeft 			= EditorGUILayout.Toggle( "\tMove From Bottom Left", effectScript.isMoveFromBottomLeft );
				}
				GUILayout.Space( spaceValue );
			}

			effectScript.isScaleChangeFoldOpen 					= EditorGUILayout.Foldout( effectScript.isScaleChangeFoldOpen , "Scale Related Effects" );
			if( effectScript.isScaleChangeFoldOpen ){
				// popup
				if( !effectScript.isCommonParams ){
					effectScript.scaleEffectSpeed 					= EditorGUILayout.FloatField( "\t\tSpeed" , effectScript.scaleEffectSpeed );
					effectScript.scaleEffectDelay 						= EditorGUILayout.FloatField( "\t\tDelay on start" , effectScript.scaleEffectDelay);
					effectScript.scaleEffectDismissDelay 			= EditorGUILayout.FloatField( "\t\tDelay on Dismiss", effectScript.scaleEffectDismissDelay );
					effectScript.isBounceBackScaleEffect 		= EditorGUILayout.Toggle( "\t\tBounce Back", effectScript.isBounceBackScaleEffect );
				}

				if( !effectScript.isReversePopUp ){
					effectScript.isPopUp 									= EditorGUILayout.Toggle( "\tPopUp", 	effectScript.isPopUp );
					if( effectScript.isPopUp ){
						effectScript.popUpStartValue 				= EditorGUILayout.Slider( "\t\tStart value", effectScript.popUpStartValue, 0f, 0.9f);
						GUILayout.Space( spaceValue );
					}
				}

			// reverse popup
				if( !effectScript.isPopUp ){
					effectScript.isReversePopUp 										= EditorGUILayout.Toggle( "\tReverse PopUp", 		effectScript.isReversePopUp );
					if( effectScript.isReversePopUp ){
						effectScript.reversePopUpStartValue 						= EditorGUILayout.Slider( "\t\tStart value", effectScript.reversePopUpStartValue, 2.0f, 25.0f);
						GUILayout.Space( spaceValue );
					}
				}
			}

			effectScript.isRotationChangeFoldOpen 			= EditorGUILayout.Foldout( effectScript.isRotationChangeFoldOpen, "Rotation related effects");
			if( effectScript.isRotationChangeFoldOpen ){
			// rotation
				effectScript.isRotation 									= EditorGUILayout.Toggle( "\tRotation", 	effectScript.isRotation );
				if( effectScript.isRotation ){
					if( !effectScript.axisX && !effectScript.axisY && !effectScript.axisZ ){
						EditorGUILayout.HelpBox("Select your axis to rotate", MessageType.Error, false );
					}
					if( !effectScript.axisY && !effectScript.axisZ ){
						effectScript.axisX = EditorGUILayout.Toggle( "\t\tX" , effectScript.axisX );
					}
					if( !effectScript.axisX && !effectScript.axisZ ){
						effectScript.axisY = EditorGUILayout.Toggle( "\t\tY" , effectScript.axisY );
					}
					if( !effectScript.axisX && !effectScript.axisY ){
						effectScript.axisZ = EditorGUILayout.Toggle( "\t\tZ" , effectScript.axisZ );
					}
					if( !effectScript.isCommonParams ){
						effectScript.rotationSpeed 							= EditorGUILayout.FloatField( "\t\tSpeed" , effectScript.rotationSpeed );
						effectScript.rotationDelay 								= EditorGUILayout.FloatField( "\t\tDelay on start" , effectScript.rotationDelay);
						effectScript.rotationDismissDelay 					= EditorGUILayout.FloatField( "\t\tDelay on Dismiss", effectScript.rotationDismissDelay );
						effectScript.isBounceBackRotation 				= EditorGUILayout.Toggle("\t\tBounce Back", effectScript.isBounceBackRotation );
					}
					effectScript.rotationDismissTime 					= EditorGUILayout.FloatField( "\t\tDismiss Time" , effectScript.rotationDismissTime );
					effectScript.flipsPerRotation 							= EditorGUILayout.IntField( "\t\tFlips per rotation" , effectScript.flipsPerRotation );
					effectScript.delayBetweenTwoRotation 			= EditorGUILayout.Slider( "\t\tDelay between two rotations" , effectScript.delayBetweenTwoRotation, 0f, 30.0f );
					effectScript.isContinuosRotation 					= EditorGUILayout.Toggle( "\t\tInfinite rotations" , effectScript.isContinuosRotation );
					if( !effectScript.isContinuosRotation ){
						effectScript.numberOfRotations 				= EditorGUILayout.IntField( "\t\tNumber of rotations" , effectScript.numberOfRotations );
					}
					GUILayout.Space( spaceValue );
				}
				GUILayout.Space( spaceValue );
			}
		}
		// check if it is click able
		if( effectScript.isClickable ){
			GUILayout.Space( spaceValue );
			EditorGUILayout.HelpBox( "Drag your game objects, that you want to dismiss on click", MessageType.Info, false );
			GUILayout.Space( spaceValue );
			base.DrawDefaultInspector();
		}

		// if gui is changed
		if( GUI.changed ){
			EditorUtility.SetDirty( effectScript );
		}
	}
}
