using UnityEngine;
using System.Collections;

public class GestureManager
{

    private GestureCamminata gCam = new GestureCamminata();
    private GestureSedutoSicurezza gSed = new GestureSedutoSicurezza();
    private GestureSaltoMovimenti gSaltoMov = new GestureSaltoMovimenti();
    private Gesture4Zampe g4zampe = new Gesture4Zampe();
    private GestureMaschera gMasc = new GestureMaschera();
    private uint UserID;

    public GestureManager()
    {
    }

    public void Init(uint id)
    {

        this.UserID = id;
        KinectWrapper.SkeletonJointTransformation piedeDx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_FOOT, ref piedeDx);
        KinectWrapper.SkeletonJointTransformation piedeSx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_FOOT, ref piedeSx);

        KinectWrapper.SkeletonJointTransformation manoDx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_FOOT, ref manoDx);
        KinectWrapper.SkeletonJointTransformation manoSx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_FOOT, ref manoSx);

        gCam.Init(piedeSx, piedeDx, UserID);
        gSaltoMov.Init(piedeSx, piedeDx, UserID);
        gSed.Init(UserID);
        g4zampe.Init(manoSx, manoDx, UserID);
        gMasc.Init(manoSx, manoDx, UserID);
    }

    public Gesture.GestureState getState()
    {
        return (gCam.transazione() | g4zampe.transazione() | gMasc.transazione() | gSed.getState() | gSaltoMov.getState());
    }

}
