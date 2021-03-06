//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's alpha.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween Number")]
public class TweenNumber : UITweener
{
	#if UNITY_3_5
	public float from = 1f;
	public float to = 1f;
	#else
		[Range(0f, 1f)]
		public float
				from = 1f;
		[Range(0f, 1f)]
		public float
				to = 1f;
	#endif
	
		UIProgressBar mRect;
	
		public UIProgressBar cachedRect {
				get {
						if (mRect == null) {
								mRect = GetComponent<UIProgressBar> ();
								if (mRect == null)
										mRect = GetComponentInChildren<UIProgressBar> ();
						}
						return mRect;
				}
		}
	
		[System.Obsolete("Use 'value' instead")]
		public float alpha { get { return this.value; } set { this.value = value; } }
	
		/// <summary>
		/// Tween's current value.
		/// </summary>
	
		public float value { get { return cachedRect.value; } set { cachedRect.value = value; } }
	
		/// <summary>
		/// Tween the value.
		/// </summary>
	
		protected override void OnUpdate (float factor, bool isFinished)
		{
				value = Mathf.Lerp (from, to, factor);
		}
	
		/// <summary>
		/// Start the tweening operation.
		/// </summary>
	
		static public TweenNumber Begin (GameObject go, float duration, float alpha)
		{
				TweenNumber comp = UITweener.Begin<TweenNumber> (go, duration);
				comp.from = comp.value;
				comp.to = alpha;
		
				if (duration <= 0f) {
						comp.Sample (1f, true);
						comp.enabled = false;
				}
				return comp;
		}
	
		public override void SetStartToCurrentValue ()
		{
				from = value;
		}
		public override void SetEndToCurrentValue ()
		{
				to = value;
		}
}
