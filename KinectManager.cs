/* Kinect Update Manager for Unity
 * 4/18/2011
 * Dave Bennett
 * 
 * This is an example of how you would write a Kinect update loop to take advantage of 
 * all the features the KinectWrapper class currently has to offer.
 * 
 * HOW TO USE:
 * 
 * Attach this script to an object present within a scene (usually the Main Camera)
 * and drag each of the models to be controlled into the appropriate lists.
 *  
 * The other game logic shoud exist out of the file.
 * 
 */

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

public class KinectManager : MonoBehaviour
{
	// Variables needed to track the users.
	KinectWrapper.UserDelegate NewUser;
	KinectWrapper.UserDelegate CalibrationStarted;
	KinectWrapper.UserDelegate CalibrationFailed;
    KinectWrapper.UserDelegate CalibrationSuccess;
    KinectWrapper.UserDelegate UserLost;
	
	// Public Bool to determine how many players there are. Default of one user.
	public bool TwoUsers = false;
	
	// Bools to keep track of who is currently calibrated.
	bool Player1Calibrated = false;
	bool Player2Calibrated = false;
	
	bool AllPlayersCalibrated = false;
	
	// Values to track which ID (assigned by the Kinect) is player 1 and player 2.
	uint Player1ID;
	uint Player2ID;
	
	// Lists of GameObjects that will be controlled by which player.
	public List<GameObject> Player1Avatars;
	public List<GameObject> Player2Avatars;
	
	// Lists of AvatarControllers that will let the models get updated.
	List<AvatarController> Player1Controllers;
	List<AvatarController> Player2Controllers;
	
	// User Map vars.
	Texture2D usersLblTex;
	Color[] usersMapColors;
	Rect usersMapRect;
	int usersMapSize;
	
	short[] usersLabelMap;
	short[] usersDepthMap;
	float[] usersHistogramMap;
	
	// List of all users
	List<uint> allUsers;

    GameObject txtTest;

	// Add model to player list.
	void AddAvatar(GameObject avatar, List<GameObject> whichPlayerList)
	{
		whichPlayerList.Add(avatar);
	}
	
	// Remove model from player list.
	void RemoveAvatar(GameObject avatar, List<GameObject> whichPlayerList)
	{
		whichPlayerList.Remove(avatar);
	}
	
	// Functions that let you recalibrate either player 1 or player 2.
	void RecalibratePlayer1()
	{
		OnUserLost(Player1ID);
	}
	
	void RecalibratePlayer2()
	{
		OnUserLost(Player2ID);
	}
	
	// When a new user enters, add it to the list.
	void OnNewUser(uint UserId)
    {
        Debug.Log(String.Format("[{0}] New user", UserId));
        allUsers.Add(UserId);
    }   
	
	// Print out when the user begins calibration.
    void OnCalibrationStarted(uint UserId)
    {
		Debug.Log(String.Format("[{0}] Calibration started", UserId));
        txtTest.guiText.text = "Rimani in questa posa senza muoverti!";
    }
	
	// Alert us when the calibration fails.
    void OnCalibrationFailed(uint UserId)
    {
        Debug.Log(String.Format("[{0}] Calibration failed", UserId));
    }
	
	// If a user successfully calibrates, assign him/her to player 1 or 2.
    void OnCalibrationSuccess(uint UserId)
    {
        Debug.Log(String.Format("[{0}] Calibration success", UserId));
		
		// If player 1 hasn't been calibrated, assign that UserID to it.
		if(!Player1Calibrated)
		{
			// Check to make sure we don't accidentally assign player 2 to player 1.
			if (UserId != Player2ID)
				{
				Player1Calibrated = true;
				Player1ID = UserId;
				
				foreach(AvatarController controller in Player1Controllers)
				{
					controller.SuccessfulCalibration();
				}
				
				// If we're not using 2 users, we're all calibrated.
				if(!TwoUsers)
				{
					AllPlayersCalibrated = true;
				}
			}
		}
		
		// Otherwise, assign to player 2.
		else
		{
			if (UserId != Player1ID)
			{
				Player2Calibrated = true;
				Player2ID = UserId;
				
				// All users are calibrated!
				AllPlayersCalibrated = true;
			}
		}
		
		// If all users are calibrated, stop trying to find them.
		if(AllPlayersCalibrated)
		{
			Debug.Log("Stopping to look for users");
			KinectWrapper.StopLookingForUsers();
		}
    }
	
	// If a user walks out of the kinects all-seeing eye, try to reassign them! Or, assign a new user to player 1.
    void OnUserLost(uint UserId)
    {
        Debug.Log(String.Format("[{0}] User lost", UserId));
		
		// If we lose player 1...
		if(UserId == Player1ID)
		{
			// Null out the ID and reset all the models associated with that ID.
			Player1ID = 0;
			
			foreach(AvatarController controller in Player1Controllers)
			{
				controller.RotateToCalibrationPose();
			}
			
			// Try to replace that user!
			Debug.Log("Starting to look for users");
			KinectWrapper.StartLookingForUsers(NewUser, CalibrationStarted, CalibrationFailed, CalibrationSuccess, UserLost);
		}
		// If we lose player 2...
		if(UserId == Player2ID)
		{
			// Null out the ID and reset all the models associated with that ID.
			Player2ID = 0;
			
			foreach(AvatarController controller in Player1Controllers)
			{
				controller.RotateToCalibrationPose();
			}
			
			// Try to replace that user!
			Debug.Log("Starting to look for users");
			KinectWrapper.StartLookingForUsers(NewUser, CalibrationStarted, CalibrationFailed, CalibrationSuccess, UserLost);
		}

        // remove from global users list
        allUsers.Remove(UserId);
    }
	
	void Start()
	{
		// Make sure we have the Open NI file.
		uint rc = KinectWrapper.Init(new StringBuilder(".\\OpenNI.xml"));
        if (rc != 0)
        {
            Debug.Log(String.Format("Error initing OpenNI: {0}", Marshal.PtrToStringAnsi(KinectWrapper.GetStatusString(rc))));
        }
		
        // Initialize depth & label map related stuff
        usersMapSize = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
        usersLblTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
        usersMapColors = new Color[usersMapSize];
        usersMapRect = new Rect(Screen.width - usersLblTex.width / 2, Screen.height - usersLblTex.height / 2, usersLblTex.width / 2, usersLblTex.height / 2);
        usersLabelMap = new short[usersMapSize];
        usersDepthMap = new short[usersMapSize];
        usersHistogramMap = new float[5000];

        // Initialize user list to contain ALL users.
        allUsers = new List<uint>();
        
        // Initialize user callbacks.
        NewUser = new KinectWrapper.UserDelegate(OnNewUser);
        CalibrationStarted = new KinectWrapper.UserDelegate(OnCalibrationStarted);
        CalibrationFailed = new KinectWrapper.UserDelegate(OnCalibrationFailed);
        CalibrationSuccess = new KinectWrapper.UserDelegate(OnCalibrationSuccess);
        UserLost = new KinectWrapper.UserDelegate(OnUserLost);
		
		// Pull the AvatarController from each of the players Avatars.
		Player1Controllers = new List<AvatarController>();
		Player2Controllers = new List<AvatarController>();
		
		// Add each of the avatars' controllers into a list for each player.
		foreach(GameObject avatar in Player1Avatars)
		{
			Player1Controllers.Add(avatar.GetComponent<AvatarController>());
		}
		
		foreach(GameObject avatar in Player2Avatars)
		{
			Player2Controllers.Add(avatar.GetComponent<AvatarController>());
		}
		
        // Start looking for users.
        KinectWrapper.StartLookingForUsers(NewUser, CalibrationStarted, CalibrationFailed, CalibrationSuccess, UserLost);
		Debug.Log("Waiting for users to calibrate");
		
		// Set the default smoothing for the Kinect.
		KinectWrapper.SetSkeletonSmoothing(0.6);

        // GUI Text.
        txtTest = GameObject.Find("txtTest");
		
	}
	
	void Update()
	{
        // Update to the next frame.
		KinectWrapper.Update(false);

        // If the players aren't all calibrated yet, draw the user map.
		if(!AllPlayersCalibrated)
		{
        	UpdateUserMap();
		}
		
		// Update player 1's models if he/she is calibrated and the model is active.
		if(Player1Calibrated)
		{
			foreach (AvatarController controller in Player1Controllers)
			{
				if(controller.Active)
				{
					controller.UpdateAvatar(Player1ID);
				}
			}
		}
		
		// Update player 2's models if he/she is calibrated and the model is active.
		if(Player2Calibrated)
		{
			foreach (AvatarController controller in Player2Controllers)
			{
				if(controller.Active)
				{
					controller.UpdateAvatar(Player2ID);
				}
			}
		}
		
		// Kill the program with ESC.
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
	
	// Make sure to kill the Kinect on quitting.
	void OnApplicationQuit()
	{
		KinectWrapper.Shutdown();
	}
	
	// Draw the Histogram Map on the GUI.
    void OnGUI()
    {
        if (!AllPlayersCalibrated)
        {
            GUI.DrawTexture(usersMapRect, usersLblTex);
        }
        else
        {
			//AllPlayersCalibrated = true;
        }
		
		// Find out if any of the currently seen users are trying to calibrate.
        foreach (uint userId in allUsers)
        {
            float progress = KinectWrapper.GetUserPausePoseProgress(userId);
            if (KinectWrapper.GetUserPausePoseProgress(userId) > 0.0)
            {
                break;
            }
        }
    }
	
	// Update / draw the User Map
    void UpdateUserMap()
    {
        // copy over the maps
        Marshal.Copy(KinectWrapper.GetUsersLabelMap(), usersLabelMap, 0, usersMapSize);
        Marshal.Copy(KinectWrapper.GetUsersDepthMap(), usersDepthMap, 0, usersMapSize);

        // Flip the texture as we convert label map to color array
        int flipIndex, i;
        int numOfPoints = 0;
		Array.Clear(usersHistogramMap, 0, usersHistogramMap.Length);

        // Calculate cumulative histogram for depth
        for (i = 0; i < usersMapSize; i++)
        {
            // Only calculate for depth that contains users
            if (usersLabelMap[i] != 0)
            {
                usersHistogramMap[usersDepthMap[i]]++;
                numOfPoints++;
            }
        }
        if (numOfPoints > 0)
        {
            for (i = 1; i < usersHistogramMap.Length; i++)
	        {   
		        usersHistogramMap[i] += usersHistogramMap[i-1];
	        }
            for (i = 0; i < usersHistogramMap.Length; i++)
	        {
                usersHistogramMap[i] = 1.0f - (usersHistogramMap[i] / numOfPoints);
	        }
        }

        // Create the actual users texture based on label map and depth histogram
        for (i = 0; i < usersMapSize; i++)
        {
            flipIndex = usersMapSize - i - 1;
            if (usersLabelMap[i] == 0)
            {
                usersMapColors[flipIndex] = Color.clear;
            }
            else
            {
                // Create a blending color based on the depth histogram
                Color c = new Color(usersHistogramMap[usersDepthMap[i]], usersHistogramMap[usersDepthMap[i]], usersHistogramMap[usersDepthMap[i]], 0.9f);
                switch (usersLabelMap[i] % 4)
                {
                    case 0:
                        usersMapColors[flipIndex] = Color.red * c;
                        break;
                    case 1:
                        usersMapColors[flipIndex] = Color.green * c;
                        break;
                    case 2:
                        usersMapColors[flipIndex] = Color.blue * c;
                        break;
                    case 3:
                        usersMapColors[flipIndex] = Color.magenta * c;
                        break;
                }
            }
        }
		
		// Draw it!
        usersLblTex.SetPixels(usersMapColors);
        usersLblTex.Apply();
    }
}


