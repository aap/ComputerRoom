using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserMovement : MonoBehaviour
{
	public float m_turnSpeed = 20;
	public InputActionProperty m_forward;
	public InputActionProperty m_turn;

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		Vector2 md = Mouse.current.delta.ReadValue();

		float z = m_forward.action.ReadValue<float>();
		transform.Translate(new Vector3(0, 0, z) * Time.deltaTime);

		float a = m_turnSpeed * m_turn.action.ReadValue<float>();
		a += md.x;
		transform.Rotate(0, a * Time.deltaTime, 0);
	}
}
