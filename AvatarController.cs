/* Kinect Avatar Controller for Unity
 * 4/11/2011
 * Dave Bennett
 * 
 * Based off of the Unity Wrapper by PrimeSense Ltd.
 * Special thanks to Shlomo Zippel for making the code this was based on.
 * Special thanks to Peter Kinney for being awesomely helpful.
 * 
 * HOW TO USE:
 * Attach this script to the avatar (within Unity) that you want the kinect to control.
 * 
 * Once attached, within Unity, drag the bones you want to be controlled by each Kinect point.
 * You can leave bones empty and they will simply not track.
 * 
 * This version will let you:
 *	- Set whether the model is active at start.
 *	- Define whether the movement should mirror the player or do the opposite.
 *	- Define whether the avatar can move in space or stay in one spot.
 *	- Set whether the model can move vertically, along with the rate of movement.
 * 
 * To define which palyer controls which model, check the KinectManager script for the main update loop.
 * 
 * NOTES:
 * Make sure the models rotation is at 0,0,0, otherwise the calibration may break.
 */

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 
using AssemblyCSharp;

public class AvatarController : MonoBehaviour
{		
	// Bool that determines whether the avatar is active.
	public bool Active = true;
	
	// Bool that has the characters (facing the player) actions become mirrored. Default true.
	public bool MirroredMovement = true;
	
	// Bool that determines whether the avatar will move or not in space.
	public bool MovesInSpace = false;

    // Bool that determines whether the avatar will rotate with the shoulder.
    public bool moveCameraShoulder = false;
	
	// Bool that determines whether the avatar is allowed to jump -- vertical movement
	// can cause some models to behave strangely, so use at your own discretion.
	public bool VerticalMovement = false;
	
	// Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
	public int MoveRate = 1;
	
	// Public variables that will get matched to bones. If empty, the kinect will simply not track it.
	// These bones can be set within the Unity interface.
	public Transform Head;
	public Transform Neck;
	public Transform Spine;
	public Transform Waist;
	
	public Transform LeftCollar;
	public Transform LeftShoulder;
	public Transform LeftElbow;
	public Transform LeftWrist;
	public Transform LeftHand;
	public Transform LeftFingertip;
	
	public Transform RightCollar;
	public Transform RightShoulder;
	public Transform RightElbow;
	public Transform RightWrist;
	public Transform RightHand;
	public Transform RightFingertip;
	
	public Transform LeftHip;
	public Transform LeftKnee;
	public Transform LeftAnkle;
	public Transform LeftFoot;
	
	public Transform RightHip;
	public Transform RightKnee;
	public Transform RightAnkle;
	public Transform RightFoot;
	
	public Transform Root;
	
	public Transform target;
	
	// A required variable if you want to rotate the model in space.
	public GameObject offsetNode;
	
	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	private Transform[] bones;
	
	// Rotations of the bones when the Kinect tracking starts.
    private Quaternion[] initialRotations;
	
	// Calibration Offset Variables for Character Position.
	bool OffsetCalibrated = false;
	float XOffset, YOffset, ZOffset;
	Quaternion originalRotation;
	
    //Gesture
    private GestureManager gManager = new GestureManager();
	private bool initGesture = false;

	// GUI Text to show messages.
	public GameObject CalibrationText;

    public void Start()
    {
        //creiamo il testo di info
        
        CalibrationText.guiText.text = "Inizio";

		// Holds our bones for later.
		bones = new Transform[(int)KinectWrapper.SkeletonJoint.END];
		
		// Initial rotations of said bones.
		initialRotations = new Quaternion[(int)KinectWrapper.SkeletonJoint.END];
		
		// Map bones to the points the Kinect tracks.
		MapBones();

		// Get initial rotations to return to later.
		GetInitialRotations();
		
		// Set the model to the calibration pose.
        RotateToCalibrationPose();
    }
	
	// Update the avatar each frame.
    public void UpdateAvatar(uint UserID)
    {
        //CalibrationText.guiText.text = "Inizio";

		// If the movement is mirrored, update all bones normally.
		if (MirroredMovement)
		{
			for (int i=0;i<bones.Length;i++)
			{
				if (bones[i] != null)
				{
					TransformBone(UserID, i, bones[i], false);
				}
			}
		}
		// Otherwise, switch each limb with the opposite one.
		else
		{
			// Update Head, Neck, Spine, and Waist normally.
			for (int i=0;i<4;i++)
			{
				if (bones[i] != null)
				{
					TransformBone(UserID, i, bones[i], true);
				}
			}
			// Beyond this, switch the arms and legs.
			
			// Left Arm --> Right Arm
			TransformBone(UserID, 4, bones[10], true);
			TransformBone(UserID, 5, bones[11], true);
			TransformBone(UserID, 6, bones[12], true);
			TransformBone(UserID, 7, bones[13], true);
			TransformBone(UserID, 8, bones[14], true);
			TransformBone(UserID, 9, bones[15], true);
			
			// Right Arm --> Left Arm
			TransformBone(UserID, 10, bones[4], true);
			TransformBone(UserID, 11, bones[5], true);
			TransformBone(UserID, 12, bones[6], true);
			TransformBone(UserID, 13, bones[7], true);
			TransformBone(UserID, 14, bones[8], true);
			TransformBone(UserID, 15, bones[9], true);
			
			// Left Leg --> Right Leg
			TransformBone(UserID, 16, bones[20], true);
			TransformBone(UserID, 17, bones[21], true);
			TransformBone(UserID, 18, bones[22], true);
			TransformBone(UserID, 19, bones[23], true);
			
			// Right Leg --> Left Leg
			TransformBone(UserID, 20, bones[16], true);
			TransformBone(UserID, 21, bones[17], true);
			TransformBone(UserID, 22, bones[18], true);
			TransformBone(UserID, 23, bones[19], true);	
		}
		
		// If the avatar is supposed to move in the space, move it.
		//if (MovesInSpace)
		//{
			MoveAvatar_my(UserID);
		//}
    }
	
	// Set bones to initial position.
    public void RotateToInitialPosition()
    {	
		// For each bone that was defined, reset to initial position.
		for (int i=0;i<bones.Length;i++)
		{
			if (bones[i] != null)
			{
				bones[i].rotation = initialRotations[i];
			}
		}
    }
	
	// Calibration pose is simply initial position with hands raised up. Rotation must be 0,0,0 to calibrate.
    public void RotateToCalibrationPose()
    {	
		// Reset the rest of the model to the original position.
        RotateToInitialPosition();
		if(bones[13] != null)
		// Right Elbow
        	bones[13].rotation = Quaternion.Euler(0, -90, 90) * initialRotations[(int)KinectWrapper.SkeletonJoint.RIGHT_ELBOW];
		
		if(bones[7] != null)
		// Left Elbow
        bones[7].rotation = Quaternion.Euler(0, 90, -90) * initialRotations[(int)KinectWrapper.SkeletonJoint.LEFT_ELBOW];
    }
	
	// On the successful calibration of a player, use this method to reset the models position.
	public void SuccessfulCalibration()
	{
		if(offsetNode!=null)
		{
			offsetNode.transform.rotation = originalRotation;
		}
	}
	
	// Apply the rotations tracked by the kinect to the joints.
    void TransformBone(uint userId, int joint, Transform dest, bool flip)
    {
		// Grab the bone we're moving.
        KinectWrapper.SkeletonJointTransformation trans = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(userId, joint, ref trans);

        // Only modify joint if confidence is high enough in this frame
        if (trans.ori.confidence > 0.5)
        {
            // Z coordinate in OpenNI is opposite from Unity. We will create a quat
            // to rotate from OpenNI to Unity (relative to initial rotation)
			Vector3 worldZVec;
			Vector3 worldYVec;
			
			if(flip)
			{
				// For the spine and waist, we flip y rotation.
				if(joint < 4)
				{
					worldZVec = new Vector3(trans.ori.m02, -trans.ori.m12, trans.ori.m22);
		            worldYVec = new Vector3(-trans.ori.m01, trans.ori.m11, -trans.ori.m21);
				}
				// Everything else, we flip in a way that doesn't break the model (MAGICAL)
				else
				{
					worldZVec = new Vector3(-trans.ori.m02, trans.ori.m12, -trans.ori.m22);
		            worldYVec = new Vector3(trans.ori.m01, -trans.ori.m11, trans.ori.m21);
				}
			}
			else
			{
	            worldZVec = new Vector3(-trans.ori.m02, -trans.ori.m12, trans.ori.m22);
	            worldYVec = new Vector3(trans.ori.m01, trans.ori.m11, -trans.ori.m21);
			}
			
			// 
            Quaternion jointRotation = Quaternion.LookRotation(worldZVec, worldYVec);
			
			// Apply the new rotation.
            Quaternion newRotation = jointRotation * initialRotations[joint];
			
			//If an offset node is specified, combine the transform with its
			//orientation to essentially make the skeleton relative to the node
			if (offsetNode != null)
			{
				// Grab the total rotation by adding the Euler and offset's Euler.
				Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
				
				// Grab our new rotation.
				newRotation = Quaternion.Euler(totalRotation);
			}
			
			// Smoothly transition to our new rotation.
	        dest.rotation = Quaternion.Slerp(dest.rotation, newRotation, Time.deltaTime * 20);
        }
	}
	
	// Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
	// Only pulls positional, not rotational.
	void MoveAvatar(uint UserID)
	{
		// Get the position of the spine and store it.
		KinectWrapper.SkeletonJointTransformation trans = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, 3, ref trans);
		
		// If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
		if (!OffsetCalibrated)
		{
			OffsetCalibrated = true;
			
			XOffset = trans.pos.x *(.001f*MoveRate);
			YOffset = trans.pos.y *(.001f*MoveRate);
			ZOffset = -trans.pos.z *(.001f*MoveRate);
			
			// Change the offsets so we can appropriately add or subtract them to the position of the model.
			if(XOffset<0)
			{ XOffset = -XOffset;	}
			if(YOffset<0)
			{ YOffset = -YOffset;	}
			if(ZOffset<0)
			{ ZOffset = -ZOffset;	}
		}
	
		float xPos;
		float yPos;
		float zPos;
		
		// If movement is mirrored, update normally. If not, reverse it.
		if(MirroredMovement)
		{
			xPos = trans.pos.x *(.001f*MoveRate) + XOffset;
		}
		else
		{
			xPos = -trans.pos.x *(.001f*MoveRate) + XOffset;
		}
		
		yPos = trans.pos.y *(.001f*MoveRate) - YOffset;
		zPos = -trans.pos.z *(.001f*MoveRate) + ZOffset;
		
		// If we are tracking vertical movement, update the y. Otherwise leave it alone.
		if(VerticalMovement)
		{
			Root.parent.localPosition = new Vector3(xPos, yPos, zPos);
		}
		else
		{
			Root.parent.localPosition = new Vector3(xPos, 0, zPos);
		}
		
	}
	
    //Funzione che gestisce l'avatar -> skeleton kinect
	void MoveAvatar_my(uint UserID){
        //Inizializzazione delle gesture con la posizione iniziale del giocatore
		if(!initGesture) {
            gManager.Init(UserID);
			initGesture = true;
		}
        //Giro il "target" in base all'angolo tra le spalle!
        if (moveCameraShoulder)
        {
            KinectWrapper.SkeletonJointTransformation spallaDx = new KinectWrapper.SkeletonJointTransformation();
            KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_SHOULDER, ref spallaDx);
            KinectWrapper.SkeletonJointTransformation spallaSx = new KinectWrapper.SkeletonJointTransformation();
            KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_SHOULDER, ref spallaSx);

            var dz = spallaDx.pos.z - spallaSx.pos.z;
            var dx = spallaDx.pos.x - spallaSx.pos.x;
            float angolo = (float)Math.Atan(dz / dx);
           
            angolo = -angolo; //Mirror
            if (Math.Abs(angolo) > 0.2)
            {
                target.transform.Rotate(0, angolo * 3.0f, 0);
            }
        }
		
		Gesture.GestureState s;
        s = gManager.getState();
        string str = "";
        CharacterController controller = target.GetComponent<CharacterController>();
        var avanti = target.transform.TransformDirection(Vector3.forward);
        float curSpeed = 0f;
        if ((s & Gesture.GestureState.Camminata) != 0)
        { 
           curSpeed = 5f;
           str += " camminata";
        }
        if ((s & Gesture.GestureState.Corsa) != 0)
        {
            curSpeed = 15f;
            str += " corsa";
        }
        controller.SimpleMove(avanti * curSpeed);

        if ((s & Gesture.GestureState.SpostamentoSX) != 0)
        {
            str += " Sinistra";
        }
        if ((s & Gesture.GestureState.SpostamentoDX) != 0)
        {
            str += " Destra";
        }
        if ((s & Gesture.GestureState.Salto) != 0)
        {
            str += " Salto";
        }
        if ((s & Gesture.GestureState.Piedi) != 0)
        {
            str += " Piedi";
        }
        if ((s & Gesture.GestureState.Seduto) != 0)
        {
            str += " Seduto";
        }
        if ((s & Gesture.GestureState.Indietro) != 0)
        {
            str += " Indietro";
        }
        if ((s & Gesture.GestureState.Cintura) != 0)
        {
            str += " Cintura";
        }
        if ((s & Gesture.GestureState.PosizioneSicurezza) != 0)
        {
            str += " Sicurezza";
        }
        if ((s & Gesture.GestureState.Gattoni) != 0)
        {
            str += " Gattoni";
        }
        if ((s & Gesture.GestureState.Maschera) != 0)
        {
            str += " Maschera";
        }

        CalibrationText.guiText.text = str;
        
	}
	
	// If the bones to be mapped have been declared, map that bone to the model.
	void MapBones()
	{
		// If they're not empty, pull in the values from Unity and assign them to the array.
		if(Head != null){bones[1] = Head;}
		if(Neck != null){bones[2] = Neck;}
		if(Spine != null){bones[3] = Spine;}
		if(Waist != null){bones[4] = Waist;}
		if(LeftCollar != null){bones[5] = LeftCollar;}
		if(LeftShoulder != null){bones[6] = LeftShoulder;}
		if(LeftElbow != null){bones[7] = LeftElbow;}
		if(LeftWrist != null){bones[8] = LeftWrist;}
		if(LeftHand != null){bones[9] = LeftHand;}
		if(LeftFingertip != null){bones[10] = LeftFingertip;}
		if(RightCollar != null){bones[11] = RightCollar;}
		if(RightShoulder != null){bones[12] = RightShoulder;}
		if(RightElbow != null){bones[13] = RightElbow;}
		if(RightWrist != null){bones[14] = RightWrist;}
		if(RightHand != null){bones[15] = RightHand;}
		if(RightFingertip != null){bones[16] = RightFingertip;}
		if(LeftHip != null){bones[17] = LeftHip;}
		if(LeftKnee != null){bones[18] = LeftKnee;}
		if(LeftAnkle != null){bones[19] = LeftAnkle;}
		if(LeftFoot != null){bones[20] = LeftFoot;}
		if(RightHip != null){bones[21] = RightHip;}
		if(RightKnee != null){bones[22] = RightKnee;}
		if(RightAnkle != null){bones[23] = RightAnkle;}
		if(RightFoot!= null){bones[24] = RightFoot;}
	}
	
	// Capture the initial rotations of the model.
	void GetInitialRotations()
	{
		if(offsetNode!=null)
		{
			// Store the original offset's rotation.
			originalRotation = offsetNode.transform.rotation;
			
			// Set the offset's rotation to 0.
			offsetNode.transform.rotation = Quaternion.Euler(Vector3.zero);
		}
		
		for (int i=0; i<bones.Length;i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation;
			}
		}
	}
}