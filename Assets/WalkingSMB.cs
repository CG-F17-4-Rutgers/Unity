using UnityEngine;

public class WalkingSMB : StateMachineBehaviour
{
	public float m_Damping = 0.15f;


	private readonly int m_HashHorizontalPara = Animator.StringToHash ("Horizontal");
	private readonly int m_HashVerticalPara = Animator.StringToHash ("Vertical");
	private readonly int m_HashStrafePara = Animator.StringToHash ("Strafe");
	private readonly int m_HashMovingPara = Animator.StringToHash ("Moving");
	private readonly int m_HashRunningPara = Animator.StringToHash ("Running");
	private readonly int m_HashStrafingPara = Animator.StringToHash ("Strafing");


	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float horizontal = Input.GetAxis ("Horizontal");
		float vertical = Input.GetAxis ("Vertical");
		float strafe = Input.GetAxis ("Strafe");

		Vector2 input = new Vector2(horizontal, vertical).normalized;

		animator.SetFloat (m_HashHorizontalPara, input.x, m_Damping, Time.deltaTime);
		animator.SetFloat (m_HashVerticalPara, input.y, m_Damping, Time.deltaTime);
		animator.SetFloat (m_HashStrafePara, strafe, m_Damping, Time.deltaTime);

		if (Mathf.Abs (vertical) > 0.1f) {
			animator.SetBool (m_HashMovingPara, true);
		} else {
			animator.SetBool (m_HashMovingPara, false);
		}
		if (Input.GetKey (KeyCode.LeftShift)) {
			animator.SetBool (m_HashRunningPara, true);
		} else {
			animator.SetBool (m_HashRunningPara, false);
		}
		if (strafe != 0.0) {
			animator.SetBool (m_HashStrafingPara, true);
		} else {
			animator.SetBool (m_HashStrafingPara, false);
		}
	}
}