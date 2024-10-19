using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MovementController : MonoBehaviour
{
	//ESCRITO POR DIEGO PEREZ RUEDA
	private class CameraFollow : MonoBehaviour
	{
		public MovementController movementController;
		public float distance;
		public float height;
		public float offset;
		private float hypotenuse;
		public float speed;
		public GameObject verticalCameraRotator;
		public GameObject horizontalCameraRotator;
		public Transform target;
		public LayerMask camCollisionMask;
		private void Start()
		{
			hypotenuse = new Vector3(offset, height, distance).magnitude;
			camCollisionMask = ~(movementController.groundExcludeMask | movementController.wallExcludeMask);
			//Debug.Log(camCollisionMask);
		}
		private void FixedUpdate()
		{
			transform.position = Utilities.FlattenVectorY(transform.position) + Utilities.ComponentVectorY(target.position);
			horizontalCameraRotator.transform.position = movementController.transform.position;
			Vector3 rayOrigin = movementController.transform.position + movementController.transform.up * movementController.colliderVerticalCenter;
			Ray ray = new Ray(rayOrigin, (target.position - rayOrigin).normalized);
			RaycastHit hit;
			//Debug.DrawRay(rayOrigin, (target.position - rayOrigin).normalized, Color.red);
			if (Physics.Raycast(ray, out hit, hypotenuse, camCollisionMask))
			{
				//Debug.Log("Hit");
				transform.position = hit.point;

				float triangleSimilarity = Vector3.Distance(transform.position, movementController.transform.position) / hypotenuse;

				transform.LookAt(movementController.transform.position + movementController.transform.up * movementController.colliderVerticalCenter + movementController.transform.up * height * triangleSimilarity + horizontalCameraRotator.transform.right * offset * triangleSimilarity);
				transform.position = transform.position + transform.forward * 0.1f;
			}
			else
			{
				if (Vector3.Distance(transform.position, movementController.transform.position) < hypotenuse)
				{
					transform.position -= (transform.position - movementController.transform.position).normalized * (Vector3.Distance(transform.position, movementController.transform.position) - hypotenuse);
				}
				Vector3 lookTarget = movementController.transform.position + movementController.transform.up * movementController.colliderVerticalCenter + movementController.transform.up * height + horizontalCameraRotator.transform.right * offset;
				transform.RotateAround(movementController.transform.position, Vector3.up, Vector3.SignedAngle(Utilities.FlattenVectorY(transform.position - lookTarget), Utilities.FlattenVectorY(target.position - lookTarget), Vector3.up));
				transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime * Vector3.Distance(transform.position, target.position) / hypotenuse);
				transform.LookAt(lookTarget);

			}
		}
	}
	private class GroundCheck : MonoBehaviour
	{
		public MovementController movementController;
		private void OnTriggerStay(Collider other)
		{
			//Debug.Log("Grounded");
			movementController.grounded = true;
			movementController.currentDoubleJumpAmount = movementController.doubleJumpAmount;
			movementController.falling = false;
		}
		private void OnTriggerExit(Collider other)
		{
			//Debug.Log("Not Grounded");
			movementController.grounded = false;
		}
	}
	private class StuckCheck : MonoBehaviour
	{
		public MovementController movementController;
		private void OnTriggerStay(Collider other)
		{
			StartCoroutine(StuckCheckTime());
			//Debug.Log("Stuck");
		}
		private void OnTriggerExit(Collider other)
		{
			movementController.stuck = false;
			StopAllCoroutines();
			//Debug.Log("Not Stuck");
		}
		private IEnumerator StuckCheckTime()
		{
			yield return new WaitForSeconds(0.1f);
			movementController.stuck = true;
		}
	}


	Camera cam;
	GameObject horizontalCameraRotator;
	GameObject verticalCameraRotator;
	GameObject cameraTarget;

	Rigidbody rigidBody;
	CapsuleCollider bodyCollider;

	float camVerticalRotation = 0;
	float camHorizontalRotation = 0;


	float currentMaxSpeed;
	float currentAcceleration;

	int currentDoubleJumpAmount;

	public bool walking => grounded && (moveDirection.magnitude > 0.5f || rigidBody.velocity.magnitude > 0.5f);

	bool grounded = true;
	bool landed = true;
	bool touchingWalls;
	bool airDashOnCooldown;
	public bool sprinting { get; private set; }
	bool readyToJump = true;
	bool falling;
	bool jumped;
	bool swinging;

	SpringJoint swingJoint;
	LineRenderer lineRenderer;

	bool inputEnabled = true;
	bool gravityEnabled = true;

	bool stuck;

	bool canDoubleJump = true;


	GroundCheck groundCheck;
	StuckCheck stuckCheck;


	Vector3 moveDirection;
	Vector3 dragDirection;
	Vector3 decelerationDirection;


	[Header("Camera Settings")]
	[SerializeField][Range(0f, 200f)] float verticalMouseSensitivity = 100f;
	[SerializeField][Range(0f, 200f)] float horizontalMouseSensitivity = 100f;
	[SerializeField][Range(0f, 90f)] float cameraMaxAngle = 80f;
	[SerializeField] float camHeight = 0;
	[SerializeField] float camDistance = 0;
	[SerializeField] float camOffset = 0;
	[SerializeField] float camSpeed = 50f;
	[SerializeField] float FOV = 60;
	[SerializeField] LayerMask cameraCullingMask = ~0;
	[Header("Collider Settings")]
	[SerializeField] float colliderHeight = 2;
	[SerializeField] float colliderRadius = 0.5f;
	[SerializeField] float colliderVerticalCenter = 0;
	[SerializeField] float groundCheckDistance = 0.1f;
	[SerializeField] LayerMask groundExcludeMask = ~0;
	[SerializeField] float wallCheckDistance = 0.1f;
	[SerializeField] LayerMask wallExcludeMask = ~0;
	[Header("Movement Settings")]
	[SerializeField] float acceleration = 50f;
	[SerializeField] float maxSpeed = 10f;
	[SerializeField] float rotationSpeed = 50f;
	[SerializeField] float drag = 5;
	[SerializeField] float gravity = 9.81f;
	[Header("Jump Settings")]
	[SerializeField] bool canJump = true;
	[SerializeField] float jumpStrength = 500f;
	[SerializeField] int doubleJumpAmount = 1;
	[SerializeField] float doubleJumpStrength = 300f;
	[SerializeField] float doubleJumpCooldown = 0.5f;
	[SerializeField] float airInfluence = 0.5f;
	[Header("Sprint Settings")]
	[SerializeField] bool canSprint = false;
	[SerializeField] float sprintRelativeSpeed = 1.5f;
	[Header("Air Dash Settings")]
	[SerializeField] bool canAirDash = true;
	[SerializeField] float airDashLength = 10f;
	[SerializeField] float airDashDuration = 0.2f;
	[SerializeField] float airDashCooldown = 2f;
	[Header("Swing Settings")]
	[SerializeField] float swingRelativeSpeed = 2f;
	[SerializeField] float swingRange = 30f;
	[SerializeField] Material lineMaterial;
	[SerializeField] LayerMask swingExcludeMask = ~0;

	[Header("References")]
	[SerializeField] Animator animator;

	private void Awake()
	{
		cam = new GameObject("PlayerCamera").AddComponent<Camera>();
		cam.tag = "MainCamera";

		horizontalCameraRotator = new GameObject("HorizontalCameraRotator");
		verticalCameraRotator = new GameObject("VerticalCameraRotator");
		cameraTarget = new GameObject("CameraTarget");

		if ((rigidBody = GetComponent<Rigidbody>()) == null)
		{
			rigidBody = gameObject.AddComponent<Rigidbody>();
		}

		if ((bodyCollider = GetComponent<CapsuleCollider>()) == null)
		{
			bodyCollider = gameObject.AddComponent<CapsuleCollider>();
			bodyCollider.radius = colliderRadius;
			bodyCollider.height = colliderHeight;
			bodyCollider.center = new Vector3(0, colliderVerticalCenter, 0);
		}
		else
		{
			colliderHeight = bodyCollider.height;
			colliderRadius = bodyCollider.radius;
			colliderVerticalCenter = bodyCollider.center.y;
		}

		lineRenderer = gameObject.AddComponent<LineRenderer>();

		groundCheck = new GameObject("GroundCheck").AddComponent<GroundCheck>();
		groundCheck.gameObject.layer = LayerMask.NameToLayer("Player");

		stuckCheck = new GameObject("StuckCheck").AddComponent<StuckCheck>();
		stuckCheck.gameObject.layer = LayerMask.NameToLayer("Player");

		cam.AddComponent<AudioListener>();

	}
	void Start()
	{
		cam.depth = 0;
		cam.cullingMask = cameraCullingMask;
		cam.useOcclusionCulling = true;
		cam.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
		cam.fieldOfView = FOV;

		cameraTarget.transform.parent = verticalCameraRotator.transform;
		cameraTarget.transform.localPosition = new Vector3(camOffset, camHeight, camDistance);
		verticalCameraRotator.transform.parent = horizontalCameraRotator.transform;
		verticalCameraRotator.transform.localPosition = Vector3.zero;
		horizontalCameraRotator.transform.position = transform.position;
		cam.transform.localPosition = new Vector3(camOffset, camHeight, camDistance);
		cam.transform.LookAt(transform.position + transform.up * camHeight + horizontalCameraRotator.transform.right * camOffset);

		CameraFollow camFollow = cam.AddComponent<CameraFollow>();
		camFollow.movementController = this;
		camFollow.distance = camDistance;
		camFollow.height = camHeight;
		camFollow.offset = camOffset;
		camFollow.verticalCameraRotator = verticalCameraRotator;
		camFollow.horizontalCameraRotator = horizontalCameraRotator;
		camFollow.speed = camSpeed;
		camFollow.target = cameraTarget.transform;

		Cursor.lockState = CursorLockMode.Locked;

		rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		rigidBody.useGravity = false;

		bodyCollider.height = colliderHeight;
		bodyCollider.radius = colliderRadius;
		bodyCollider.center = new Vector3(0, colliderVerticalCenter, 0);

		PhysicMaterial physicsMaterial = new PhysicMaterial(); 
		physicsMaterial.bounciness = 0;
		physicsMaterial.dynamicFriction = 0;
		physicsMaterial.staticFriction = 0;
		physicsMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
		physicsMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
		bodyCollider.material = physicsMaterial;

		groundCheck.movementController = this;
		groundCheck.transform.parent = transform;
		groundCheck.transform.position = new Vector3(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y, bodyCollider.bounds.center.z);
		BoxCollider groundColl = groundCheck.AddComponent<BoxCollider>();
		groundColl.size = new Vector3(colliderRadius / 10, groundCheckDistance, colliderRadius / 10);
		groundColl.isTrigger = true;
		groundColl.excludeLayers = groundExcludeMask;
		Physics.IgnoreCollision(groundColl, bodyCollider);

		stuckCheck.movementController = this;
		stuckCheck.transform.parent = transform;
		stuckCheck.transform.position = new Vector3(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y, bodyCollider.bounds.center.z);
		CapsuleCollider stuckColl = stuckCheck.AddComponent<CapsuleCollider>();
		stuckColl.radius = colliderRadius * 0.85f;
		stuckColl.height = colliderHeight * 0.85f;
		stuckColl.isTrigger = true;
		stuckColl.excludeLayers = groundExcludeMask;
		Physics.IgnoreCollision(stuckColl, bodyCollider);
		Physics.IgnoreCollision(stuckColl, groundColl);

		currentAcceleration = acceleration;
		currentMaxSpeed = maxSpeed;

		lineRenderer.material = lineMaterial;
		lineRenderer.startWidth = 0.1f;
		lineRenderer.endWidth = 0.1f;
	}
	void Update()
	{
		CheckInput();
		MoveBody();
		StateHandler();
	}
	private void FixedUpdate()
	{
		if (stuck)
		{
			rigidBody.position = rigidBody.position + Vector3.up;
			//Debug.Log("Stuck");
		}
		Gravity();
		MovePlayer();
		
	}
	private void CheckInput()
	{
		//Debug.Log(grounded);
		if (!inputEnabled)
			return;

		camHorizontalRotation += Input.GetAxis("Mouse X") * horizontalMouseSensitivity / 100;
		camVerticalRotation -= Input.GetAxis("Mouse Y") * verticalMouseSensitivity / 100; ;
		camVerticalRotation = Mathf.Clamp(camVerticalRotation, -cameraMaxAngle, cameraMaxAngle);

		horizontalCameraRotator.transform.localRotation = Quaternion.Euler(0, camHorizontalRotation, 0);
		verticalCameraRotator.transform.localRotation = Quaternion.Euler(camVerticalRotation, 0, 0);

		if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
		{
			moveDirection = (horizontalCameraRotator.transform.forward * Input.GetAxis("Vertical") + horizontalCameraRotator.transform.right * Input.GetAxis("Horizontal")).normalized * currentAcceleration;
		}
		else
		{
			moveDirection = Vector3.zero;
		}

		if(Input.GetButtonDown("Fire1") && !grounded)
		{
			Ray ray = new Ray(cam.transform.position, cam.transform.forward);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, swingRange, ~swingExcludeMask))
			{
				Swing(hit);
			}
		}
		if(Input.GetButtonUp("Fire1") || grounded)
		{
			StopSwing();
		}

		if (Input.GetButtonDown("Jump") && canJump)
		{
			if (grounded)
			{
				if (readyToJump)
				{
					animator.SetTrigger("Windup");
					readyToJump = false;
					StartCoroutine(JumpWindUp(new Vector3(0, jumpStrength, 0), 15));
				}
			}
			else if (currentDoubleJumpAmount > 0 && canDoubleJump)
			{
				canDoubleJump = false;
				Invoke(nameof(CanDoubleJump), doubleJumpCooldown);
				StopSwing();
				Jump(new Vector3(0, doubleJumpStrength, 0));
				currentDoubleJumpAmount--;
			}
		}
		if (Input.GetButton("Sprint") && canSprint && grounded)
		{
			//Debug.Log("Sprint");
			sprinting = true;
			currentAcceleration = acceleration * sprintRelativeSpeed;
			currentMaxSpeed = maxSpeed * sprintRelativeSpeed;
		}
		else if(Input.GetButtonDown("Sprint") && canAirDash && !grounded && !airDashOnCooldown)
		{
			StopSwing();
			//Debug.Log("Air Dash");
			rigidBody.velocity = Vector3.zero;
			StartCoroutine(AirDash());
		}
		else if(Input.GetButtonUp("Sprint") && canSprint || !grounded)
		{
			sprinting = false;
			currentAcceleration = acceleration;
			currentMaxSpeed = maxSpeed;
		}
	}
	private void StateHandler()
	{
		//animator.SetFloat("Speed", rigidBody.velocity.magnitude * 0.1f);
		//Debug.Log(animator.GetBool("Walking"));
		animator.SetBool("Sprinting", sprinting);
		animator.SetFloat("Angle", Vector3.SignedAngle(transform.forward, rigidBody.velocity.normalized, transform.up));
		animator.SetBool("Walking", grounded && (moveDirection.magnitude > 0.5f || rigidBody.velocity.magnitude > 0.5f));
		if (grounded)
		{
			if (!landed)
			{
				landed = true;
				animator.SetTrigger("Land");
				Invoke(nameof(ReadyToJump), 0.3f);
				//Debug.Log("Land");
			}
		}
		else
		{
			landed = false;
		}
	}
	private void MovePlayer()
	{
		if(!inputEnabled)
			return;

		decelerationDirection = Utilities.FlattenVectorY(rigidBody.velocity) * -currentAcceleration / currentMaxSpeed;
		if (Mathf.Approximately(moveDirection.magnitude, 0))
		{
			dragDirection = Utilities.FlattenVectorY(rigidBody.velocity) * -drag;
		}
		else
		{
			dragDirection = Vector3.zero;
		}

        if (grounded)
			rigidBody.AddForce((moveDirection + dragDirection + decelerationDirection) * Time.fixedDeltaTime, ForceMode.VelocityChange);
		else if(!swinging)
			rigidBody.AddForce((moveDirection + dragDirection + decelerationDirection) * airInfluence * Time.fixedDeltaTime, ForceMode.VelocityChange);
		else
			rigidBody.AddForce((moveDirection) * airInfluence * Time.fixedDeltaTime, ForceMode.VelocityChange);

		if(moveDirection.magnitude > 0)
			rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), rotationSpeed * Time.fixedDeltaTime));

			
		if (!grounded && !falling && !jumped)
		{
			falling = true;
			animator.ResetTrigger("Land");
			readyToJump = false;
			//Debug.Log("Fall");
			animator.SetTrigger("Fall");
		}
	}
	private void Jump(Vector3 impulse)
	{
		rigidBody.velocity = Utilities.FlattenVectorY(rigidBody.velocity);
		rigidBody.AddForce(impulse, ForceMode.Acceleration);
	}
	private void Gravity()
	{
		if(gravityEnabled)
			rigidBody.AddForce(Vector3.up * -gravity, ForceMode.Acceleration);
	}
	void ReadyToJump()
	{
		jumped = false;
		readyToJump = true;
	}
	IEnumerator JumpWindUp(Vector3 jumpVector, int framesToWait)
	{
		jumped = true;
		//Debug.Log("Jump");
		for(int i = 0; i < framesToWait; i++)
		{
			yield return new WaitForEndOfFrame();
		}
		animator.SetTrigger("Jump");
		Jump(jumpVector);
		animator.ResetTrigger("Land");
		
	}
	IEnumerator AirDash()
	{
		Vector3 dashDirection = cam.transform.forward;
		RaycastHit hit;
		Ray ray = new Ray(transform.position + transform.up * colliderVerticalCenter, dashDirection);
		float currentAirDashLength = airDashLength;
		if(Physics.Raycast(ray, out hit, airDashLength, ~(wallExcludeMask | groundExcludeMask)))
		{
			currentAirDashLength = (hit.point - (transform.position + transform.up * colliderVerticalCenter)).magnitude - (wallCheckDistance * 2 + colliderRadius);
			if (currentAirDashLength < 1 || touchingWalls)
				yield break;
		}
		airDashOnCooldown = true;
		Invoke(nameof(DashCooldown), airDashCooldown);
		//Debug.Log(currentAirDashLength);
		animator.SetBool("Dash", true);
		DeactivateInput();
		gravityEnabled = false;
		rigidBody.rotation = Quaternion.LookRotation(dashDirection);
		for(float i = 0; i < airDashDuration; i += Time.deltaTime)
		{
			float distance = currentAirDashLength * Time.deltaTime / airDashDuration;
			if (Physics.CapsuleCast(transform.position + transform.up * (bodyCollider.radius + wallCheckDistance), transform.position + transform.up * (bodyCollider.height - bodyCollider.radius + wallCheckDistance), bodyCollider.radius + wallCheckDistance, dashDirection, out hit, distance * 2, ~(groundExcludeMask | wallExcludeMask)))
			{
				//Debug.Log(hit.collider.gameObject.name);
				break;
			}
			else
			{
				rigidBody.position = rigidBody.position + verticalCameraRotator.transform.forward * distance;
				yield return new WaitForEndOfFrame();
			}
		}
		rigidBody.rotation = Quaternion.LookRotation(Utilities.FlattenVectorY(dashDirection).normalized, Vector3.up);
		rigidBody.velocity = Vector3.zero;
		gravityEnabled = true;
		ActivateInput();
		animator.SetBool("Dash", false);
	}
	void Swing(RaycastHit hit)
	{
		switch(Random.Range(0, 2))
		{
			case 0:
				SoundManager.Instance.PlaySound(SoundManager.Sounds.SWING_1, transform.position);
				break;
			case 1:
				SoundManager.Instance.PlaySound(SoundManager.Sounds.SWING_2, transform.position);
				break;
		}
		

		currentDoubleJumpAmount = doubleJumpAmount;
		currentMaxSpeed = maxSpeed * swingRelativeSpeed;
		animator.SetBool("Swinging", true);
		swinging = true;
		swingJoint = gameObject.AddComponent<SpringJoint>();
		swingJoint.autoConfigureConnectedAnchor = false;
		swingJoint.connectedAnchor = hit.point;

		float distance = Vector3.Distance(transform.position, hit.point);
		swingJoint.maxDistance = distance * 0.8f;
		swingJoint.minDistance = 0;

		swingJoint.spring = 4.5f;
		swingJoint.damper = 7f;
		swingJoint.massScale = 4.5f;

		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(1, hit.point);
	}
	void StopSwing()
	{
		currentMaxSpeed = maxSpeed;
		animator.SetBool("Swinging", false);
		Destroy(swingJoint);
		swinging = false;
		lineRenderer.positionCount = 0;
	}
	void DrawLine()
	{
		lineRenderer.SetPosition(0, animator.gameObject.transform.position + animator.gameObject.transform.forward * 0.5f);
	}
	void DashCooldown()
	{
		airDashOnCooldown = false;
	}
	void MoveBody()
	{
		if (!swinging)
		{
			animator.gameObject.transform.position = transform.position;
			animator.gameObject.transform.rotation = Quaternion.Lerp(animator.gameObject.transform.rotation, transform.rotation, rotationSpeed);
		}
		else
		{
			Vector3 up = swingJoint.connectedAnchor - transform.position;
			Vector3 cross = Vector3.Cross(up, -transform.right);
			animator.gameObject.transform.rotation = Quaternion.LookRotation(cross, up);
			animator.gameObject.transform.position = transform.position + animator.gameObject.transform.up * colliderVerticalCenter;
			DrawLine();
			if (Physics.Raycast(animator.gameObject.transform.position, animator.transform.up, out RaycastHit hit, 1, ~swingExcludeMask))
			{
				StopSwing();
			}
		}
	}
	void CanDoubleJump()
	{
		canDoubleJump = true;
	}
	public void DeactivateInput()
	{
		inputEnabled = false;
	}
	public void ActivateInput()
	{
		inputEnabled = true;
	}
}
