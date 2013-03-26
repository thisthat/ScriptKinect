using System;

public class GestureSaltoMovimenti : Gesture
{
    //User ID
    private uint UserID;
    private KinectWrapper.SkeletonJointPosition ps_init;	//Sinistro di init
    private KinectWrapper.SkeletonJointPosition pd_init;	//Destro di init

    private float delta_laterale = 50f;
    private float delta_salto = 150;
    private float delta_indietro = 300;
    private float delta_cintura = 150;
    private float min_p = 0;
    private float max_p = 20;

    public GestureSaltoMovimenti()
        : base()
    {
    }
    public void Init(KinectWrapper.SkeletonJointTransformation s, KinectWrapper.SkeletonJointTransformation d, uint id)
    {
        this.pd_init = d.pos;
        this.ps_init = s.pos;
        this.UserID = id;
    }
    public override GestureState transazione()
    {
        return getState();
    }

    public GestureState getState()
    {
        GestureState ret = GestureState.Null;
        //Info dei point di joint che ci interessano
        KinectWrapper.SkeletonJointTransformation piedeDx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.SkeletonJointTransformation piedeSx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_FOOT, ref piedeDx);
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_FOOT, ref piedeSx);

        //Laterale Destro
        if (piedeDx.pos.x < (pd_init.x - delta_laterale) && piedeSx.pos.x < (ps_init.x - delta_laterale))
        {
            pd_init.x = piedeDx.pos.x;
            ps_init.x = piedeSx.pos.x;
            ret = ret | GestureState.SpostamentoDX;
        }
        //Laterale Sinistro
        if (piedeDx.pos.x > (pd_init.x + delta_laterale) && piedeSx.pos.x > (ps_init.x + delta_laterale))
        {
            pd_init.x = piedeDx.pos.x;
            ps_init.x = piedeSx.pos.x;
            ret = ret | GestureState.SpostamentoSX;
        }
        //Salto
        if (Math.Abs(piedeDx.pos.y - pd_init.y) >  delta_salto && Math.Abs(piedeSx.pos.y - ps_init.y) > delta_salto)
        {
            ret = ret | GestureState.Salto;
        }
        //Piede davanti all'altro
        if (Math.Abs(piedeDx.pos.z - piedeSx.pos.z) > delta_indietro)
        {
            ret = ret | GestureState.Indietro;
        }
        //cintura
        KinectWrapper.SkeletonJointTransformation manoSx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_HAND, ref manoSx);
        KinectWrapper.SkeletonJointTransformation manoDx = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_HAND, ref manoDx);
        KinectWrapper.SkeletonJointTransformation torso = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.TORSO_CENTER, ref torso);
        KinectWrapper.SkeletonJointTransformation testa = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.HEAD, ref testa);

        int h = (int)testa.pos.y;
        float perc_s = Math.Abs(manoSx.pos.y / h * 100);
        float perc_d = Math.Abs(manoDx.pos.y / h * 100);
        float x_dx,x_sx;
        x_dx = Math.Abs(manoDx.pos.x - torso.pos.x);
        x_sx = Math.Abs(manoSx.pos.x - torso.pos.x);
        
        if (
            0 <= x_dx && x_dx <= delta_cintura && 0 <= x_sx && x_sx <= delta_cintura && //Controllo sull'asse x
            min_p <= perc_s && perc_s <= max_p && min_p <= perc_d && perc_d <= max_p //controllo asse y
            )
        {
            ret = ret | GestureState.Cintura;
        }
        return ret;
    }
}


