using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class HackerMovement : MonoBehaviour
{
	public InputActionProperty m_leftStick, m_rightStick;
	public float m_rotateSensitivity= 10.0f;
	public float m_moveSensitivity = 1.0f;
	public GameObject m_head;

	public InputActionProperty m_leftGrip, m_rightGrip;
	public InputActionProperty m_primary, m_secondary;

	void Start()
	{
	}

	void Update()
	{
		Vector2 moveInput = m_rightStick.action.ReadValue<Vector2>();
		Vector2 rotateInput = m_leftStick.action.ReadValue<Vector2>();

		Vector3 fwd = m_head.transform.forward;
		fwd.y = 0.0f;
		fwd.Normalize();
		Vector3 right = m_head.transform.right;
		right.y = 0.0f;
		right.Normalize();

		transform.Rotate(0.0f, rotateInput.x*m_rotateSensitivity*Time.deltaTime, 0.0f);
		transform.position += fwd*moveInput.y*m_moveSensitivity + right*moveInput.x*m_moveSensitivity;
	}


	// taken from ActionBasedController
	float m_ButtonPressPoint = 0.5f;
        protected bool IsPressed(InputAction action)
        {
            if (action == null)
                return false;

#if INPUT_SYSTEM_1_1_OR_NEWER
                return action.phase == InputActionPhase.Performed;
#else
            if (action.activeControl is ButtonControl buttonControl)
                return buttonControl.isPressed;

            if (action.activeControl is AxisControl)
                return action.ReadValue<float>() >= m_ButtonPressPoint;

            return action.triggered || action.phase == InputActionPhase.Performed;
#endif
        }
}
