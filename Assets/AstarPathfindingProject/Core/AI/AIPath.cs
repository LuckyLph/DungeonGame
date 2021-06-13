	/// <summary>
	/// AI for following paths.
	/// This AI is the default movement script which comes with the A* Pathfinding Project.
	/// It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
	/// to set up movement for the characters in your game.
	/// This script works well for many types of units, but if you need the highest performance (for example if you are moving hundreds of characters) you
	/// may want to customize this script or write a custom movement script to be able to optimize it specifically for your game.
	///
	/// This script will try to move to a given <see cref="destination"/>. At <see cref="repathRate regular"/>, the path to the destination will be recalculated.
	/// If you want to make the AI to follow a particular object you can attach the <see cref="Pathfinding.AIDestinationSetter"/> component.
	/// Take a look at the getstarted (view in online documentation for working links) tutorial for more instructions on how to configure this script.
	///
	/// Here is a video of this script being used move an agent around (technically it uses the <see cref="Pathfinding.Examples.MineBotAI"/> script that inherits from this one but adds a bit of animation support for the example scenes):
	/// [Open online documentation to see videos]
	///
	/// \section variables Quick overview of the variables
	/// In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.
	///
	/// The <see cref="repathRate"/> determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.
	/// The <see cref="destination"/> field is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
	/// Or it can be the player object in a zombie game.
	/// The <see cref="maxSpeed"/> is self-explanatory, as is <see cref="rotationSpeed"/>. however <see cref="slowdownDistance"/> might require some explanation:
	/// It is the approximate distance from the target where the AI will start to slow down. Setting it to a large value will make the AI slow down very gradually.
	/// <see cref="pickNextWaypointDist"/> determines the distance to the point the AI will move to (see image below).
	///
	/// Below is an image illustrating several variables that are exposed by this class (<see cref="pickNextWaypointDist"/>, <see cref="steeringTarget"/>, <see cref="desiredVelocity)"/>
	/// [Open online documentation to see images]
	///
	/// This script has many movement fallbacks.
	/// If it finds an RVOController attached to the same GameObject as this component, it will use that. If it finds a character controller it will also use that.
	/// If it finds a rigidbody it will use that. Lastly it will fall back to simply modifying Transform.position which is guaranteed to always work and is also the most performant option.
	///
	/// \section how-aipath-works How it works
	/// In this section I'm going to go over how this script is structured and how information flows.
	/// This is useful if you want to make changes to this script or if you just want to understand how it works a bit more deeply.
	/// However you do not need to read this section if you are just going to use the script as-is.
	///
	/// This script inherits from the <see cref="AIBase"/> class. The movement happens either in Unity's standard <see cref="Update"/> or <see cref="FixedUpdate"/> method.
	/// They are both defined in the AIBase class. Which one is actually used depends on if a rigidbody is used for movement or not.
	/// Rigidbody movement has to be done inside the FixedUpdate method while otherwise it is better to do it in Update.
	///
	/// From there a call is made to the <see cref="MovementUpdate"/> method (which in turn calls <see cref="MovementUpdateInternal)"/>.
	/// This method contains the main bulk of the code and calculates how the AI *wants* to move. However it doesn't do any movement itself.
	/// Instead it returns the position and rotation it wants the AI to move to have at the end of the frame.
	/// The <see cref="Update"/> (or <see cref="FixedUpdate)"/> method then passes these values to the <see cref="FinalizeMovement"/> method which is responsible for actually moving the character.
	/// That method also handles things like making sure the AI doesn't fall through the ground using raycasting.
	///
	/// The AI recalculates its path regularly. This happens in the Update method which checks <see cref="shouldRecalculatePath"/> and if that returns true it will call <see cref="SearchPath"/>.
	/// The <see cref="SearchPath"/> method will prepare a path request and send it to the <see cref="Pathfinding.Seeker"/> component which should be attached to the same GameObject as this script.
	/// Since this script will when waking up register to the <see cref="Pathfinding.Seeker.pathCallback"/> delegate this script will be notified every time a new path is calculated by the <see cref="OnPathComplete"/> method being called.
	/// It may take one or sometimes multiple frames for the path to be calculated, but finally the <see cref="OnPathComplete"/> method will be called and the current path that the AI is following will be replaced.
	/// </summary>
