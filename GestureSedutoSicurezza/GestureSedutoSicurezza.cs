using System;

public class GestureSedutoSicurezza : Gesture
{


    //User ID
    private uint UserID;

    float delta_sicurezza = 250;

    public GestureSedutoSicurezza()
        : base()
    {
    }
    public void Init(uint id)
    {
        this.UserID = id;
    }
    public override GestureState transazione()
    {
        return getState();
    }

    public GestureState getState()
    {
        //Piedi / Seduto
        KinectWrapper.SkeletonJointTransformation torso = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.TORSO_CENTER, ref torso);
        GestureState ret = GestureState.Null;
        if (torso.pos.y < 50)
        {
            ret = ret | GestureState.Seduto;
        }
        else
        {
            ret = ret | GestureState.Piedi;
        }

        //Sicurezza
        KinectWrapper.SkeletonJointTransformation testa = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.HEAD, ref testa);
        KinectWrapper.SkeletonJointTransformation manoSx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_HAND, ref manoSx);
        KinectWrapper.SkeletonJointTransformation manoDx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_HAND, ref manoDx);


        if (Math.Abs(Math.Abs(manoDx.pos.x) - Math.Abs(testa.pos.x)) < delta_sicurezza && Math.Abs(Math.Abs(manoSx.pos.x) - Math.Abs(testa.pos.x)) < delta_sicurezza && //x
            Math.Abs(Math.Abs(manoDx.pos.y) - Math.Abs(testa.pos.y)) < delta_sicurezza && Math.Abs(Math.Abs(manoSx.pos.y) - Math.Abs(testa.pos.y)) < delta_sicurezza && //y
            Math.Abs(Math.Abs(manoDx.pos.z) - Math.Abs(testa.pos.z)) < delta_sicurezza && Math.Abs(Math.Abs(manoSx.pos.z) - Math.Abs(testa.pos.z)) < delta_sicurezza //z
            && (ret & GestureState.Seduto) != 0
            )
        {
            ret = ret | GestureState.PosizioneSicurezza;
        }

        return ret;
    }
}


