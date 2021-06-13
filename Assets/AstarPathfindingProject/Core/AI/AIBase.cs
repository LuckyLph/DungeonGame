using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Pathfinding {
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/// <summary>
	/// Base class for AIPath and RichAI.
	/// This class holds various methods and fields that are common to both AIPath and RichAI.
	///
	/// See: <see cref="Pathfinding.AIPath"/>
	/// See: <see cref="Pathfinding.RichAI"/>
	/// See: <see cref="Pathfinding.IAstarAI"/> (all movement scripts implement this interface)
	/// </summary>
	[RequireComponent(typeof(Seeker))]
	public abstract class AIBase : MonoBehaviour, IAstarAI {
		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		public float radius = 0.5f;

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		public float height = 2;

		/// <summary>
		/// Determines how often the agent will search for new paths (in seconds).
		/// The agent will plan a new path to the target every N seconds.
		///
		/// If you have fast moving targets or AIs, you might want to set it to a lower value.
		///
		/// See: <see cref="shouldRecalculatePath"/>
		/// See: <see cref="SearchPath"/>
		/// </summary>
		public float repathRate = 0.5f;

		/// <summary>\copydoc Pathfinding::IAstarAI::canSearch</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("repeatedlySearchPaths")]
		public bool canSearch = true;

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		public bool canMove = true;

		/// <summary>Max speed in world units per second</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("speed")]
		public float maxSpeed = 1;

		/// <summary>
		/// Gravity to use.
		/// If set to (NaN,NaN,NaN) then Physics.Gravity (configured in the Unity project settings) will be used.
		/// If set to (0,0,0) then no gravity will be used and no raycast to check for ground penetration will be performed.
		/// </summary>
		public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);

		/// <summary>
		/// Layer mask to use for ground placement.
		/// Make sure this does not include the layer of any colliders attached to this gameobject.
		///
		/// See: <see cref="gravity"/>
		/// See: https://docs.unity3d.com/Manual/Layers.html
		/// </summary>
		public LayerMask groundMask = -1;

		/// <summary>
		/// Offset along the Y coordinate for the ground raycast start position.
		/// Normally the pivot of the character is at the character's feet, but you usually want to fire the raycast
		/// from the character's center, so this value should be half of the character's height.
		///
		/// A green gizmo line will be drawn upwards from the pivot point of the character to indicate where the raycast will start.
		///
		/// See: <see cref="gravity"/>
		/// Deprecated: Use the <see cref="height"/> property instead (2x this value)
		/// </summary>
		[System.Obsolete("Use the height property instead (2x this value)")]
		public float centerOffset {
			get { return height * 0.5f; } set { height = value * 2; }
		}

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("centerOffset")]
		float centerOffsetCompatibility = float.NaN;

		/// <summary>
		/// Determines which direction the agent moves in.
		/// For 3D games you most likely want the ZAxisIsForward option as that is the convention for 3D games.
		/// For 2D games you most likely want the YAxisIsForward option as that is the convention for 2D games.
		///
		/// Using the YAxisForward option will also allow the agent to assume that the movement will happen in the 2D (XY) plane instead of the XZ plane
		/// if it does not know. This is important only for the point graph which does not have a well defined up direction. The other built-in graphs (e.g the grid graph)
		/// will all tell the agent which movement plane it is supposed to use.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("rotationIn2D")]
		public OrientationMode orientation = OrientationMode.ZAxisForward;

		/// <summary>
		/// If true, the forward axis of the character will be along the Y axis instead of the Z axis.
		///
		/// Deprecated: Use <see cref="orientation"/> instead
		/// </summary>
		[System.Obsolete("Use orientation instead")]
		public bool rotationIn2D {
			get { return orientation == OrientationMode.YAxisForward; }
			set { orientation = value ? OrientationMode.YAxisForward : OrientationMode.ZAxisForward; }
		}

		/// <summary>
		/// If true, the AI will rotate to face the movement direction.
		/// See: <see cref="orientation"/>
		/// </summary>
		public bool enableRotation = true;

		/// <summary>
		/// Position of the agent.
		/// If <see cref="updatePosition"/> is true then this value will be synchronized every frame with Transform.position.
		/// </summary>
		protected Vector3 simulatedPosition;

		/// <summary>
		/// Rotation of the agent.
		/// If <see cref="updateRotation"/> is true then this value will be synchronized every frame with Transform.rotation.
		/// </summary>
		protected Quaternion simulatedRotation;

		/// <summary>
		/// Position of the agent.
		/// In world space.
		/// If <see cref="updatePosition"/> is true then this value is idential to transform.position.
		/// See: <see cref="Teleport"/>
		/// See: <see cref="Move"/>
		/// </summary>
		public Vector3 position { get { return updatePosition ? tr.position : simulatedPosition; } }

		/// <summary>
		/// Rotation of the agent.
		/// If <see cref="updateRotation"/> is true then this value is identical to transform.rotation.
		/// </summary>
		public Quaternion rotation { get { return updateRotation ? tr.rotation : simulatedRotation; } }

		/// <summary>Accumulated movement deltas from the <see cref="Move"/> method</summary>
		Vector3 accumulatedMovementDelta = Vector3.zero;

		/// <summary>
		/// Current desired velocity of the agent (does not include local avoidance and physics).
		/// Lies in the movement plane.
		/// </summary>
		protected Vector2 velocity2D;

		/// <summary>
		/// Velocity due to gravity.
		/// Perpendicular to the movement plane.
		///
		/// When the agent is grounded this may not accurately reflect the velocity of the agent.
		/// It may be non-zero even though the agent is not moving.
		/// </summary>
		protected float verticalVelocity;

		/// <summary>Cached Seeker component</summary>
		protected Seeker seeker;

		/// <summary>Cached Transform component</summary>
		protected Transform tr;

		/// <summary>Cached Rigidbody component</summary>
		protected Rigidbody rigid;

		/// <summary>Cached Rigidbody component</summary>
		protected Rigidbody2D rigid2D;

		/// <summary>Cached CharacterController component</summary>
		protected CharacterController controller;


		/// <summary>
		/// Plane which this agent is moving in.
		/// This is used to convert between world space and a movement plane to make it possible to use this script in
		/// both 2D games and 3D games.
		/// </summary>
		public IMovementPlane movementPlane = GraphTransform.identityTransform;

		/// <summary>
		/// Determines if the character's position should be coupled to the Transform's position.
		/// If false then all movement calculations will happen as usual, but the object that this component is attached to will not move
		/// instead only the <see cref="position"/> property will change.
		///
		/// This is useful if you want to control the movement of the character using some other means such
		/// as for example root motion but still want the AI to move freely.
		/// See: Combined with calling <see cref="MovementUpdate"/> from a separate script instead of it being called automatically one can take a similar approach to what is documented here: https://docs.unity3d.com/Manual/nav-CouplingAnimationAndNavigation.html
		///
		/// See: <see cref="canMove"/> which in contrast to this field will disable all movement calculations.
		/// See: <see cref="updateRotation"/>
		/// </summary>
		[System.NonSerialized]
		public bool updatePosition = true;

		/// <summary>
		/// Determines if the character's rotation should be coupled to the Transform's rotation.
		/// If false then all movement calculations will happen as usual, but the object that this component is attached to will not rotate
		/// instead only the <see cref="rotation"/> property will change.
		///
		/// See: <see cref="updatePosition"/>
		/// </summary>
		[System.NonSerialized]
		public bool updateRotation = true;

		/// <summary>Indicates if gravity is used during this frame</summary>
		protected bool usingGravity { get; set; }

		/// <summary>Delta time used for movement during the last frame</summary>
		protected float lastDeltaTime;

		/// <summary>Last frame index when <see cref="prevPosition1"/> was updated</summary>
		protected int prevFrame;

		/// <summary>Position of the character at the end of the last frame</summary>
		protected Vector3 prevPosition1;

		/// <summary>Position of the character at the end of the frame before the last frame</summary>
		protected Vector3 prevPosition2;

		/// <summary>Amount which the character wants or tried to move with during the last frame</summary>
		protected Vector2 lastDeltaPosition;

		/// <summary>Only when the previous path has been calculated should the script consider searching for a new path</summary>
		protected bool waitingForPathCalculation = false;

		/// <summary>Time when the last path request was started</summary>
		protected float lastRepath = float.NegativeInfinity;

		[UnityEngine.Serialization.FormerlySerializedAs("target")][SerializeField][HideInInspector]
		Transform targetCompatibility;

		/// <summary>
		/// True if the Start method has been executed.
		/// Used to test if coroutines should be started in OnEnable to prevent calculating paths
		/// in the awake stage (or rather before start on frame 0).
		/// </summary>
		bool startHasRun = false;

		/// <summary>
		/// Target to move towards.
		/// The AI will try to follow/move towards this target.
		/// It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
		///
		/// Deprecated: In 4.1 this will automatically add a <see cref="Pathfinding.AIDestinationSetter"/> component and set the target on that component.
		/// Try instead to use the <see cref="destination"/> property which does not require a transform to be created as the target or use
		/// the AIDestinationSetter component directly.
		/// </summary>
		[System.Obsolete("Use the destination property or the AIDestinationSetter component instead")]
		public Transform target {
			get {
				var setter = GetComponent<AIDestinationSetter>();
				return setter != null ? setter.target : null;
			}
			set {
				targetCompatibility = null;
				var setter = GetComponent<AIDestinationSetter>();
				if (setter == null) setter = gameObject.AddComponent<AIDestinationSetter>();
				setter.target = value;
				destination = value != null ? value.position : new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::destination</summary>
		public Vector3 destination { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::velocity</summary>
		public Vector3 velocity {
			get {
				return lastDeltaTime > 0.000001f ? (prevPosition1 - prevPosition2) / lastDeltaTime : Vector3.zero;
			}
		}

		/// <summary>
		/// Velocity that this agent wants to move with.
		/// Includes gravity and local avoidance if applicable.
		/// </summary>
		public Vector3 desiredVelocity { get { return lastDeltaTime > 0.00001f ? movementPlane.ToWorld(lastDeltaPosition / lastDeltaTime, verticalVelocity) : Vector3.zero; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::isStopped</summary>
		public bool isStopped { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::onSearchPath</summary>
		public System.Action onSearchPath { get; set; }

		/// <summary>True if the path should be automatically recalculated as soon as possible</summary>
		protected virtual bool shouldRecalculatePath {
			get {
				return Time.time - lastRepath >= repathRate && !waitingForPathCalculation && canSearch && !float.IsPositiveInfinity(destination.x);
			}
		}

		protected AIBase () {
			// Note that this needs to be set here in the constructor and not in e.g Awake
			// because it is possible that other code runs and sets the destination property
			// before the Awake method on this script runs.
			destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		/// <summary>
		/// Looks for any attached components like RVOController and CharacterController etc.
		///
		/// This is done during <see cref="OnEnable"/>. If you are adding/removing components during runtime you may want to call this function
		/// to make sure that this script finds them. It is unfortunately prohibitive from a performance standpoint to look for components every frame.
		/// </summary>
		public virtual void FindComponents () {
			tr = transform;
			seeker = GetComponent<Seeker>();
			// Find attached movement components
			controller = GetComponent<CharacterController>();
			rigid = GetComponent<Rigidbody>();
			rigid2D = GetComponent<Rigidbody2D>();
		}

		/// <summary>Called when the component is enabled</summary>
		protected virtual void OnEnable () {
			FindComponents();
			// Make sure we receive callbacks when paths are calculated
			seeker.pathCallback += OnPathComplete;
			Init();
		}

		/// <summary>
		/// Starts searching for paths.
		/// If you virtual this method you should in most cases call base.Start () at the start of it.
		/// See: <see cref="Init"/>
		/// </summary>
		protected virtual void Start () {
			startHasRun = true;
			Init();
		}

		void Init () {
			if (startHasRun) {
				// Clamp the agent to the navmesh (which is what the Teleport call will do essentially. Though only some movement scripts require this, like RichAI).
				// The Teleport call will also make sure some variables are properly initialized (like #prevPosition1 and #prevPosition2)
				Teleport(position, false);
				lastRepath = float.NegativeInfinity;
				if (shouldRecalculatePath) SearchPath();
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::Teleport</summary>
		public virtual void Teleport (Vector3 newPosition, bool clearPath = true) {
			reachedEndOfPath = false;
			if (clearPath) ClearPath();
			prevPosition1 = prevPosition2 = simulatedPosition = newPosition;
			if (updatePosition) tr.position = newPosition;
			if (clearPath) SearchPath();
		}

		protected void CancelCurrentPathRequest () {
			waitingForPathCalculation = false;
			// Abort calculation of the current path
			if (seeker != null) seeker.CancelCurrentPathRequest();
		}

		protected virtual void OnDisable () {
			ClearPath();

			// Make sure we no longer receive callbacks when paths complete
			seeker.pathCallback -= OnPathComplete;

			velocity2D = Vector3.zero;
			accumulatedMovementDelta = Vector3.zero;
			verticalVelocity = 0f;
			lastDeltaTime = 0;
			// Release current path so that it can be pooled
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);
		}

		/// <summary>
		/// Called every frame.
		/// If no rigidbodies are used then all movement happens here.
		/// </summary>
		protected virtual void Update () {
			if (shouldRecalculatePath) SearchPath();

			// If gravity is used depends on a lot of things.
			// For example when a non-kinematic rigidbody is used then the rigidbody will apply the gravity itself
			// Note that the gravity can contain NaN's, which is why the comparison uses !(a==b) instead of just a!=b.
			usingGravity = !(gravity == Vector3.zero) && (!updatePosition || ((rigid == null || rigid.isKinematic) && (rigid2D == null || rigid2D.isKinematic)));
			if (rigid == null && rigid2D == null && canMove) {
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/// <summary>
		/// Called every physics update.
		/// If rigidbodies are used then all movement happens here.
		/// </summary>
		protected virtual void FixedUpdate () {
			if (!(rigid == null && rigid2D == null) && canMove) {
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.fixedDeltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::MovementUpdate</summary>
		public void MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			lastDeltaTime = deltaTime;
			MovementUpdateInternal(deltaTime, out nextPosition, out nextRotation);
		}

		/// <summary>
		/// Outputs the start point and end point of the next automatic path request.
		/// This is a separate method to make it easy for subclasses to swap out the endpoints
		/// of path requests. For example the <see cref="LocalSpaceRichAI"/> script which requires the endpoints
		/// to be transformed to graph space first.
		/// </summary>
		protected virtual void CalculatePathRequestEndpoints (out Vector3 start, out Vector3 end) {
			start = GetFeetPosition();
			end = destination;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::SearchPath</summary>
		public virtual void SearchPath () {
			if (float.IsPositiveInfinity(destination.x)) return;
			if (onSearchPath != null) onSearchPath();

			lastRepath = Time.time;
			waitingForPathCalculation = true;

			seeker.CancelCurrentPathRequest();

			Vector3 start, end;
			CalculatePathRequestEndpoints(out start, out end);

			// Alternative way of requesting the path
			//ABPath p = ABPath.Construct(start, end, null);
			//seeker.StartPath(p);

			// This is where we should search to
			// Request a path to be calculated from our current position to the destination
			seeker.StartPath(start, end);
		}

		/// <summary>
		/// Position of the base of the character.
		/// This is used for pathfinding as the character's pivot point is sometimes placed
		/// at the center of the character instead of near the feet. In a building with multiple floors
		/// the center of a character may in some scenarios be closer to the navmesh on the floor above
		/// than to the floor below which could cause an incorrect path to be calculated.
		/// To solve this the start point of the requested paths is always at the base of the character.
		/// </summary>
		public virtual Vector3 GetFeetPosition () {
			return position;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::SetPath</summary>
		public void SetPath (Path path) {
			if (path == null) {
				CancelCurrentPathRequest();
				ClearPath();
			} else if (path.PipelineState == PathState.Created) {
				// Path has not started calculation yet
				lastRepath = Time.time;
				waitingForPathCalculation = true;
				seeker.CancelCurrentPathRequest();
				seeker.StartPath(path);
			} else if (path.PipelineState == PathState.Returned) {
				// Path has already been calculated

				// We might be calculating another path at the same time, and we don't want that path to virtual this one. So cancel it.
				if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
				else throw new System.ArgumentException("If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");

				OnPathComplete(path);
			} else {
				// Path calculation has been started, but it is not yet complete. Cannot really handle this.
				throw new System.ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
			}
		}

		/// <summary>
		/// Accelerates the agent downwards.
		/// See: <see cref="verticalVelocity"/>
		/// See: <see cref="gravity"/>
		/// </summary>
		protected void ApplyGravity (float deltaTime) {
			// Apply gravity
			if (usingGravity) {
				float verticalGravity;
				velocity2D += movementPlane.ToPlane(deltaTime * (float.IsNaN(gravity.x) ? Physics.gravity : gravity), out verticalGravity);
				verticalVelocity += verticalGravity;
			} else {
				verticalVelocity = 0;
			}
		}

		/// <summary>Calculates how far to move during a single frame</summary>
		protected Vector2 CalculateDeltaToMoveThisFrame (Vector2 position, float distanceToEndOfPath, float deltaTime) {
			// Direction and distance to move during this frame
			return Vector2.ClampMagnitude(velocity2D * deltaTime, distanceToEndOfPath);
		}

		/// <summary>
		/// Simulates rotating the agent towards the specified direction and returns the new rotation.
		///
		/// Note that this only calculates a new rotation, it does not change the actual rotation of the agent.
		/// Useful when you are handling movement externally using <see cref="FinalizeMovement"/> but you want to use the built-in rotation code.
		///
		/// See: <see cref="orientation"/>
		/// </summary>
		/// <param name="direction">Direction in world space to rotate towards.</param>
		/// <param name="maxDegrees">Maximum number of degrees to rotate this frame.</param>
		public Quaternion SimulateRotationTowards (Vector3 direction, float maxDegrees) {
			return SimulateRotationTowards(movementPlane.ToPlane(direction), maxDegrees);
		}

		/// <summary>
		/// Simulates rotating the agent towards the specified direction and returns the new rotation.
		///
		/// Note that this only calculates a new rotation, it does not change the actual rotation of the agent.
		///
		/// See: <see cref="orientation"/>
		/// See: <see cref="movementPlane"/>
		/// </summary>
		/// <param name="direction">Direction in the movement plane to rotate towards.</param>
		/// <param name="maxDegrees">Maximum number of degrees to rotate this frame.</param>
		protected Quaternion SimulateRotationTowards (Vector2 direction, float maxDegrees) {
			if (direction != Vector2.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(movementPlane.ToWorld(direction, 0), movementPlane.ToWorld(Vector2.zero, 1));
				// This causes the character to only rotate around the Z axis
				if (orientation == OrientationMode.YAxisForward) targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.RotateTowards(simulatedRotation, targetRotation, maxDegrees);
			}
			return simulatedRotation;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::Move</summary>
		public virtual void Move (Vector3 deltaPosition) {
			accumulatedMovementDelta += deltaPosition;
		}

		/// <summary>
		/// Moves the agent to a position.
		///
		/// This is used if you want to virtual how the agent moves. For example if you are using
		/// root motion with Mecanim.
		///
		/// This will use a CharacterController, Rigidbody, Rigidbody2D or the Transform component depending on what options
		/// are available.
		///
		/// The agent will be clamped to the navmesh after the movement (if such information is available, generally this is only done by the RichAI component).
		///
		/// See: <see cref="MovementUpdate"/> for some example code.
		/// See: <see cref="controller"/>, <see cref="rigid"/>, <see cref="rigid2D"/>
		/// </summary>
		/// <param name="nextPosition">New position of the agent.</param>
		/// <param name="nextRotation">New rotation of the agent. If #enableRotation is false then this parameter will be ignored.</param>
		public virtual void FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation) {
			if (enableRotation) FinalizeRotation(nextRotation);
			FinalizePosition(nextPosition);
		}

		void FinalizeRotation (Quaternion nextRotation) {
			simulatedRotation = nextRotation;
			if (updateRotation) {
				if (rigid != null) rigid.MoveRotation(nextRotation);
				else if (rigid2D != null) rigid2D.MoveRotation(nextRotation.eulerAngles.z);
				else tr.rotation = nextRotation;
			}
		}

		void FinalizePosition (Vector3 nextPosition) {
			// Use a local variable, it is significantly faster
			Vector3 currentPosition = simulatedPosition;
			bool positionDirty1 = false;

			if (controller != null && controller.enabled && updatePosition) {
				// Use CharacterController
				// The Transform may not be at #position if it was outside the navmesh and had to be moved to the closest valid position
				tr.position = currentPosition;
				controller.Move((nextPosition - currentPosition) + accumulatedMovementDelta);
				// Grab the position after the movement to be able to take physics into account
				// TODO: Add this into the clampedPosition calculation below to make RVO better respond to physics
				currentPosition = tr.position;
				if (controller.isGrounded) verticalVelocity = 0;
			} else {
				// Use Transform, Rigidbody, Rigidbody2D or nothing at all (if updatePosition = false)
				float lastElevation;
				movementPlane.ToPlane(currentPosition, out lastElevation);
				currentPosition = nextPosition + accumulatedMovementDelta;

				// Position the character on the ground
				if (usingGravity) currentPosition = RaycastPosition(currentPosition, lastElevation);
				positionDirty1 = true;
			}

			// Clamp the position to the navmesh after movement is done
			bool positionDirty2 = false;
			currentPosition = ClampToNavmesh(currentPosition, out positionDirty2);

			// Assign the final position to the character if we haven't already set it (mostly for performance, setting the position can be slow)
			if ((positionDirty1 || positionDirty2) && updatePosition) {
				// Note that rigid.MovePosition may or may not move the character immediately.
				// Check the Unity documentation for the special cases.
				if (rigid != null) rigid.MovePosition(currentPosition);
				else if (rigid2D != null) rigid2D.MovePosition(currentPosition);
				else tr.position = currentPosition;
			}

			accumulatedMovementDelta = Vector3.zero;
			simulatedPosition = currentPosition;
			UpdateVelocity();
		}

		protected void UpdateVelocity () {
			var currentFrame = Time.frameCount;

			if (currentFrame != prevFrame) prevPosition2 = prevPosition1;
			prevPosition1 = position;
			prevFrame = currentFrame;
		}

		/// <summary>
		/// Checks if the character is grounded and prevents ground penetration.
		///
		/// Sets <see cref="verticalVelocity"/> to zero if the character is grounded.
		///
		/// Returns: The new position of the character.
		/// </summary>
		/// <param name="position">Position of the character in the world.</param>
		/// <param name="lastElevation">Elevation coordinate before the agent was moved. This is along the 'up' axis of the #movementPlane.</param>
		protected Vector3 RaycastPosition (Vector3 position, float lastElevation) {
			RaycastHit hit;
			float elevation;

			movementPlane.ToPlane(position, out elevation);
			float rayLength = tr.localScale.y * height * 0.5f + Mathf.Max(0, lastElevation-elevation);
			Vector3 rayOffset = movementPlane.ToWorld(Vector2.zero, rayLength);

			if (Physics.Raycast(position + rayOffset, -rayOffset, out hit, rayLength, groundMask, QueryTriggerInteraction.Ignore)) {
				// Grounded
				// Make the vertical velocity fall off exponentially. This is reasonable from a physical standpoint as characters
				// are not completely stiff and touching the ground will not immediately negate all velocity downwards. The AI will
				// stop moving completely due to the raycast penetration test but it will still *try* to move downwards. This helps
				// significantly when moving down along slopes as if the vertical velocity would be set to zero when the character
				// was grounded it would lead to a kind of 'bouncing' behavior (try it, it's hard to explain). Ideally this should
				// use a more physically correct formula but this is a good approximation and is much more performant. The constant
				// '5' in the expression below determines how quickly it converges but high values can lead to too much noise.
				verticalVelocity *= System.Math.Max(0, 1 - 5 * lastDeltaTime);
				return hit.point;
			}
			return position;
		}

		protected virtual void OnDrawGizmosSelected () {
			// When selected in the Unity inspector it's nice to make the component react instantly if
			// any other components are attached/detached or enabled/disabled.
			// We don't want to do this normally every frame because that would be expensive.
			if (Application.isPlaying) FindComponents();
			if (!alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		public static readonly Color ShapeGizmoColor = new Color(240/255f, 213/255f, 30/255f);

		protected virtual void OnDrawGizmos () {
			if (!Application.isPlaying || !enabled) FindComponents();

			var color = ShapeGizmoColor;
			if (orientation == OrientationMode.YAxisForward) {
				Draw.Gizmos.Cylinder(position, Vector3.forward, 0, radius * tr.localScale.x, color);
			} else {
				Draw.Gizmos.Cylinder(position, rotation * Vector3.up, tr.localScale.y * height, radius * tr.localScale.x, color);
			}

			if (!float.IsPositiveInfinity(destination.x) && Application.isPlaying) Draw.Gizmos.CircleXZ(destination, 0.2f, Color.blue);
			if (alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		protected virtual void Reset () {
			ResetShape();
			base.Reset();
		}

		void ResetShape () {
			var cc = GetComponent<CharacterController>();

			if (cc != null) {
				radius = cc.radius;
				height = Mathf.Max(radius*2, cc.height);
			}
		}

		protected virtual int OnUpgradeSerializedData (int version, bool unityThread) {
			if (unityThread && !float.IsNaN(centerOffsetCompatibility)) {
				height = centerOffsetCompatibility*2;
				ResetShape();
				centerOffsetCompatibility = float.NaN;
			}
			#pragma warning disable 618
			if (unityThread && targetCompatibility != null) target = targetCompatibility;
			#pragma warning restore 618
			// Approximately convert from a damping value to a degrees per second value.
			if (version < 1) rotationSpeed *= 90;
			return 2;
		}
		/// <summary>
		/// How quickly the agent accelerates.
		/// Positive values represent an acceleration in world units per second squared.
		/// Negative values are interpreted as an inverse time of how long it should take for the agent to reach its max speed.
		/// For example if it should take roughly 0.4 seconds for the agent to reach its max speed then this field should be set to -1/0.4 = -2.5.
		/// For a negative value the final acceleration will be: -acceleration*maxSpeed.
		/// This behaviour exists mostly for compatibility reasons.
		///
		/// In the Unity inspector there are two modes: Default and Custom. In the Default mode this field is set to -2.5 which means that it takes about 0.4 seconds for the agent to reach its top speed.
		/// In the Custom mode you can set the acceleration to any positive value.
		/// </summary>
		public float maxAcceleration = -2.5f;

		/// <summary>
		/// Rotation speed in degrees per second.
		/// Rotation is calculated using Quaternion.RotateTowards. This variable represents the rotation speed in degrees per second.
		/// The higher it is, the faster the character will be able to rotate.
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("turningSpeed")]
		public float rotationSpeed = 360;

		/// <summary>Distance from the end of the path where the AI will start to slow down</summary>
		public float slowdownDistance = 0.6F;

		/// <summary>
		/// How far the AI looks ahead along the path to determine the point it moves to.
		/// In world units.
		/// If you enable the <see cref="alwaysDrawGizmos"/> toggle this value will be visualized in the scene view as a blue circle around the agent.
		/// [Open online documentation to see images]
		///
		/// Here are a few example videos showing some typical outcomes with good values as well as how it looks when this value is too low and too high.
		/// <table>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-danger">Too low</span><br/></verbatim>\endxmlonly A too low value and a too low acceleration will result in the agent overshooting a lot and not managing to follow the path well.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-warning">Ok</span><br/></verbatim>\endxmlonly A low value but a high acceleration works decently to make the AI follow the path more closely. Note that the <see cref="Pathfinding.AILerp"/> component is better suited if you want the agent to follow the path without any deviations.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-success">Ok</span><br/></verbatim>\endxmlonly A reasonable value in this example.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-success">Ok</span><br/></verbatim>\endxmlonly A reasonable value in this example, but the path is followed slightly more loosely than in the previous video.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-danger">Too high</span><br/></verbatim>\endxmlonly A too high value will make the agent follow the path too loosely and may cause it to try to move through obstacles.</td></tr>
		/// </table>
		/// </summary>
		public float pickNextWaypointDist = 2;

		/// <summary>
		/// Distance to the end point to consider the end of path to be reached.
		/// When the end is within this distance then <see cref="OnTargetReached"/> will be called and <see cref="reachedEndOfPath"/> will return true.
		/// </summary>
		public float endReachedDistance = 0.2F;

		/// <summary>Draws detailed gizmos constantly in the scene view instead of only when the agent is selected and settings are being modified</summary>
		public bool alwaysDrawGizmos;

		/// <summary>
		/// Slow down when not facing the target direction.
		/// Incurs at a small performance overhead.
		/// </summary>
		public bool slowWhenNotFacingTarget = true;

		/// <summary>
		/// What to do when within <see cref="endReachedDistance"/> units from the destination.
		/// The character can either stop immediately when it comes within that distance, which is useful for e.g archers
		/// or other ranged units that want to fire on a target. Or the character can continue to try to reach the exact
		/// destination point and come to a full stop there. This is useful if you want the character to reach the exact
		/// point that you specified.
		///
		/// Note: <see cref="reachedEndOfPath"/> will become true when the character is within <see cref="endReachedDistance"/> units from the destination
		/// regardless of what this field is set to.
		/// </summary>
		public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;

		/// <summary>
		/// Ensure that the character is always on the traversable surface of the navmesh.
		/// When this option is enabled a <see cref="AstarPath.GetNearest"/> query will be done every frame to find the closest node that the agent can walk on
		/// and if the agent is not inside that node, then the agent will be moved to it.
		///
		/// This is especially useful together with local avoidance in order to avoid agents pushing each other into walls.
		/// See: local-avoidance (view in online documentation for working links) for more info about this.
		///
		/// This option also integrates with local avoidance so that if the agent is say forced into a wall by other agents the local avoidance
		/// system will be informed about that wall and can take that into account.
		///
		/// Enabling this has some performance impact depending on the graph type (pretty fast for grid graphs, slightly slower for navmesh/recast graphs).
		/// If you are using a navmesh/recast graph you may want to switch to the <see cref="Pathfinding.RichAI"/> movement script which is specifically written for navmesh/recast graphs and
		/// does this kind of clamping out of the box. In many cases it can also follow the path more smoothly around sharp bends in the path.
		///
		/// It is not recommended that you use this option together with the funnel modifier on grid graphs because the funnel modifier will make the path
		/// go very close to the border of the graph and this script has a tendency to try to cut corners a bit. This may cause it to try to go slightly outside the
		/// traversable surface near corners and that will look bad if this option is enabled.
		///
		/// Warning: This option makes no sense to use on point graphs because point graphs do not have a surface.
		/// Enabling this option when using a point graph will lead to the agent being snapped to the closest node every frame which is likely not what you want.
		///
		/// Below you can see an image where several agents using local avoidance were ordered to go to the same point in a corner.
		/// When not constraining the agents to the graph they are easily pushed inside obstacles.
		/// [Open online documentation to see images]
		/// </summary>
		public bool constrainInsideGraph = false;

		/// <summary>Current path which is followed</summary>
		protected Path path;

		/// <summary>Helper which calculates points along the current path</summary>
		protected PathInterpolator interpolator = new PathInterpolator();

		#region IAstarAI implementation

		/// <summary>\copydoc Pathfinding::IAstarAI::remainingDistance</summary>
		public float remainingDistance
		{
			get
			{
				return interpolator.valid ? interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - position).magnitude : float.PositiveInfinity;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedDestination</summary>
		public bool reachedDestination
		{
			get
			{
				if (!reachedEndOfPath) return false;
				if (remainingDistance + movementPlane.ToPlane(destination - interpolator.endPoint).magnitude > endReachedDistance) return false;

				// Don't do height checks in 2D mode
				if (orientation != OrientationMode.YAxisForward)
				{
					// Check if the destination is above the head of the character or far below the feet of it
					float yDifference;
					movementPlane.ToPlane(destination - position, out yDifference);
					var h = tr.localScale.y * height;
					if (yDifference > h || yDifference < -h * 0.5) return false;
				}

				return true;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedEndOfPath</summary>
		public bool reachedEndOfPath { get; protected set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::hasPath</summary>
		public bool hasPath
		{
			get
			{
				return interpolator.valid;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::pathPending</summary>
		public bool pathPending
		{
			get
			{
				return waitingForPathCalculation;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
		public Vector3 steeringTarget
		{
			get
			{
				return interpolator.valid ? interpolator.position : position;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		float IAstarAI.radius { get { return radius; } set { radius = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		float IAstarAI.height { get { return height; } set { height = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::maxSpeed</summary>
		float IAstarAI.maxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::canSearch</summary>
		bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

		#endregion

		/// <summary>\copydoc Pathfinding::IAstarAI::GetRemainingPath</summary>
		public void GetRemainingPath(List<Vector3> buffer, out bool stale)
		{
			buffer.Clear();
			buffer.Add(position);
			if (!interpolator.valid)
			{
				stale = true;
				return;
			}

			stale = false;
			interpolator.GetRemainingPath(buffer);
		}

		/// <summary>
		/// The end of the path has been reached.
		/// If you want custom logic for when the AI has reached it's destination add it here. You can
		/// also create a new script which inherits from this one and virtual the function in that script.
		///
		/// This method will be called again if a new path is calculated as the destination may have changed.
		/// So when the agent is close to the destination this method will typically be called every <see cref="repathRate"/> seconds.
		/// </summary>
		public virtual void OnTargetReached()
		{
		}

		/// <summary>
		/// Called when a requested path has been calculated.
		/// A path is first requested by <see cref="UpdatePath"/>, it is then calculated, probably in the same or the next frame.
		/// Finally it is returned to the seeker which forwards it to this function.
		/// </summary>
		protected virtual void OnPathComplete(Path newPath)
		{
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			waitingForPathCalculation = false;

			// Increase the reference count on the new path.
			// This is used for object pooling to reduce allocations.
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error)
			{
				p.Release(this);
				return;
			}

			// Release the previous path.
			if (path != null) path.Release(this);

			// Replace the old path
			path = p;

			// Make sure the path contains at least 2 points
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolator.SetPath(path.vectorPath);

			var graph = path.path.Count > 0 ? AstarData.GetGraph(path.path[0]) as ITransformedGraph : null;
			movementPlane = graph != null ? graph.transform : (orientation == OrientationMode.YAxisForward ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

			// Reset some variables
			reachedEndOfPath = false;

			// Simulate movement from the point where the path was requested
			// to where we are right now. This reduces the risk that the agent
			// gets confused because the first point in the path is far away
			// from the current position (possibly behind it which could cause
			// the agent to turn around, and that looks pretty bad).
			interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

			// Update which point we are moving towards.
			// Note that we need to do this here because otherwise the remainingDistance field might be incorrect for 1 frame.
			// (due to interpolator.remainingDistance being incorrect).
			interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

			var distanceToEnd = remainingDistance;
			if (distanceToEnd <= endReachedDistance)
			{
				reachedEndOfPath = true;
				OnTargetReached();
			}
		}

		protected virtual void ClearPath()
		{
			CancelCurrentPathRequest();
			interpolator.SetPath(null);
			reachedEndOfPath = false;
		}

		/// <summary>Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not</summary>
		protected virtual void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
		{
			float currentAcceleration = maxAcceleration;

			// If negative, calculate the acceleration from the max speed
			if (currentAcceleration < 0) currentAcceleration *= -maxSpeed;

			if (updatePosition)
			{
				// Get our current position. We read from transform.position as few times as possible as it is relatively slow
				// (at least compared to a local variable)
				simulatedPosition = tr.position;
			}
			if (updateRotation) simulatedRotation = tr.rotation;

			var currentPosition = simulatedPosition;

			// Update which point we are moving towards
			interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
			var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

			// Calculate the distance to the end of the path
			float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

			// Check if we have reached the target
			var prevTargetReached = reachedEndOfPath;
			reachedEndOfPath = distanceToEnd <= endReachedDistance && interpolator.valid;
			if (!prevTargetReached && reachedEndOfPath) OnTargetReached();
			float slowdown;

			// Normalized direction of where the agent is looking
			var forwards = movementPlane.ToPlane(simulatedRotation * (orientation == OrientationMode.YAxisForward ? Vector3.up : Vector3.forward));

			// Check if we have a valid path to follow and some other script has not stopped the character
			if (interpolator.valid && !isStopped)
			{
				// How fast to move depending on the distance to the destination.
				// Move slower as the character gets closer to the destination.
				// This is always a value between 0 and 1.
				slowdown = distanceToEnd < slowdownDistance ? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

				if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop)
				{
					// Slow down as quickly as possible
					velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
				}
				else
				{
					velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized * maxSpeed, velocity2D, currentAcceleration, rotationSpeed, maxSpeed, forwards) * deltaTime;
				}
			}
			else
			{
				slowdown = 1;
				// Slow down as quickly as possible
				velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
			}

			velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, slowdown, slowWhenNotFacingTarget && enableRotation, forwards);

			ApplyGravity(deltaTime);


			// Set how much the agent wants to move during this frame
			var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
			nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * lastDeltaTime);
			CalculateNextRotation(slowdown, out nextRotation);
		}

		protected virtual void CalculateNextRotation(float slowdown, out Quaternion nextRotation)
		{
			if (lastDeltaTime > 0.00001f && enableRotation)
			{
				Vector2 desiredRotationDirection;
				desiredRotationDirection = velocity2D;

				// Rotate towards the direction we are moving in.
				// Don't rotate when we are very close to the target.
				var currentRotationSpeed = rotationSpeed * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
				nextRotation = SimulateRotationTowards(desiredRotationDirection, currentRotationSpeed * lastDeltaTime);
			}
			else
			{
				// TODO: simulatedRotation
				nextRotation = rotation;
			}
		}

		static NNConstraint cachedNNConstraint = NNConstraint.Default;
		protected virtual Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
		{
			if (constrainInsideGraph)
			{
				cachedNNConstraint.tags = seeker.traversableTags;
				cachedNNConstraint.graphMask = seeker.graphMask;
				cachedNNConstraint.distanceXZ = true;
				var clampedPosition = AstarPath.active.GetNearest(position, cachedNNConstraint).position;

				// We cannot simply check for equality because some precision may be lost
				// if any coordinate transformations are used.
				var difference = movementPlane.ToPlane(clampedPosition - position);
				float sqrDifference = difference.sqrMagnitude;
				if (sqrDifference > 0.001f * 0.001f)
				{
					// The agent was outside the navmesh. Remove that component of the velocity
					// so that the velocity only goes along the direction of the wall, not into it
					velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

					positionChanged = true;
					// Return the new position, but ignore any changes in the y coordinate from the ClampToNavmesh method as the y coordinates in the navmesh are rarely very accurate
					return position + movementPlane.ToWorld(difference);
				}
			}

			positionChanged = false;
			return position;
		}

#if UNITY_EDITOR
		[System.NonSerialized]
		int gizmoHash = 0;

		[System.NonSerialized]
		float lastChangedTime = float.NegativeInfinity;

		protected static readonly Color GizmoColor = new Color(46.0f / 255, 104.0f / 255, 201.0f / 255);

		void OnDrawGizmosInternal()
		{
			var newGizmoHash = pickNextWaypointDist.GetHashCode() ^ slowdownDistance.GetHashCode() ^ endReachedDistance.GetHashCode();

			if (newGizmoHash != gizmoHash && gizmoHash != 0) lastChangedTime = Time.realtimeSinceStartup;
			gizmoHash = newGizmoHash;
			float alpha = alwaysDrawGizmos ? 1 : Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - lastChangedTime - 5f) / 0.5f) * (UnityEditor.Selection.gameObjects.Length == 1 ? 1 : 0);

			if (alpha > 0)
			{
				// Make sure the scene view is repainted while the gizmos are visible
				if (!alwaysDrawGizmos) UnityEditor.SceneView.RepaintAll();
				Draw.Gizmos.Line(position, steeringTarget, GizmoColor * new Color(1, 1, 1, alpha));
				Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation * (orientation == OrientationMode.YAxisForward ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity), Vector3.one);
				Draw.Gizmos.CircleXZ(Vector3.zero, pickNextWaypointDist, GizmoColor * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, slowdownDistance, Color.Lerp(GizmoColor, Color.red, 0.5f) * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, endReachedDistance, Color.Lerp(GizmoColor, Color.red, 0.8f) * new Color(1, 1, 1, alpha));
			}
		}
#endif
	}
}
