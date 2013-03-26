using System;

class GestureMaschera : Gesture
{

    //User ID
    private uint UserID;
    //Vecchia posizione delle mani
    private KinectWrapper.SkeletonJointPosition ms;	//Sinistro
    private KinectWrapper.SkeletonJointPosition md;	//Destro

    private KinectWrapper.SkeletonJointPosition ms_init;	//Sinistro di init
    private KinectWrapper.SkeletonJointPosition md_init;	//Destro di init

    private double _TIMER = 1750;
    private int delta = 250;
    private float delta_distanza = 300;
    private float delta_distanza_testa = 150;
    //Posizioni attuali delle mani e testa
    KinectWrapper.SkeletonJointTransformation manoDx = new KinectWrapper.SkeletonJointTransformation();
    KinectWrapper.SkeletonJointTransformation manoSx = new KinectWrapper.SkeletonJointTransformation();
    KinectWrapper.SkeletonJointTransformation testa = new KinectWrapper.SkeletonJointTransformation();

    private DateTime newStatus; //Serve anche per eseguire e-transazioni verso lo stato di init se non si fa niente per tot tempo 

    private enum Stati
    {
        Q0 = 0, //init
        Q1,
        Q2,
        Q3,
        Q4,
        Q5,
        Q6,
        Q7,
        Q8,
        Q9
    }
    private Stati state = Stati.Q0; //Stato attuale

    public GestureMaschera()
        : base()
    {

    }
    public void Init(KinectWrapper.SkeletonJointTransformation s, KinectWrapper.SkeletonJointTransformation d, uint id)
    {
        this.md = this.md_init = d.pos;
        this.ms = this.ms_init = s.pos;
        this.UserID = id;
    }
    public string e;
    public override Gesture.GestureState transazione()
    {
        //Info dei point di joint che ci interessano
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_HAND, ref manoSx);
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_HAND, ref manoDx);
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.HEAD, ref testa);

        if (this.state == Stati.Q0)
        {
            if (distanza(manoDx.pos, manoSx.pos) > delta_distanza)
            {
                this.state = Stati.Q1;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q1)
        {
            //Mano dx davanti al viso
            if (this.davantiDx())
            {
                this.state = Stati.Q2;
                newStatus = DateTime.Now;
            }
            //Mano sx davanti al viso
            if (this.davantiSx())
            {
                this.state = Stati.Q6;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q2)
        {
            if (this.sopraDx())
            {
                this.state = Stati.Q3;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q3)
        {
            if (this.dietroDx())
            {
                this.state = Stati.Q4;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q4)
        {
            if (this.davantiSx() && distanza(testa.pos, manoSx.pos) < delta_distanza_testa)
            {
                this.state = Stati.Q5;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q6)
        {
            if (this.sopraSx())
            {
                this.state = Stati.Q7;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q7)
        {
            if (this.dietroSx())
            {
                this.state = Stati.Q8;
                newStatus = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q8)
        {
            if (this.davantiDx() && distanza(testa.pos, manoDx.pos) < delta_distanza_testa)
            {
                this.state = Stati.Q9;
                newStatus = DateTime.Now;
            }
        }
        //Scade il timer
        if (checkTimer())
        {
            this.state = Stati.Q0;
        }

        return getState();
    }

    public GestureState getState()
    {
        switch (this.state)
        {
            case Stati.Q5:
            case Stati.Q9: return GestureState.Maschera;
            default: return GestureState.Null;
        }
    }

    private bool checkTimer()
    {
        return (DateTime.Now.Subtract(newStatus).TotalMilliseconds > this._TIMER);
    }


    private bool davantiDx()
    {
        return (Math.Abs(manoDx.pos.y - testa.pos.y) < delta && Math.Abs(manoDx.pos.x - testa.pos.x) < delta && manoDx.pos.z < testa.pos.z);
    }
    private bool davantiSx()
    {
        return (Math.Abs(manoSx.pos.y - testa.pos.y) < delta && Math.Abs(manoSx.pos.x - testa.pos.x) < delta && manoSx.pos.z < testa.pos.z);
    }

    private bool sopraDx()
    {
        return (Math.Abs(manoDx.pos.z - testa.pos.z) < delta && Math.Abs(manoDx.pos.x - testa.pos.x) < delta && manoDx.pos.y > testa.pos.y);
    }
    private bool sopraSx()
    {
        return (Math.Abs(manoSx.pos.z - testa.pos.z) < delta && Math.Abs(manoSx.pos.x - testa.pos.x) < delta && manoSx.pos.y > testa.pos.y);
    }

    private bool dietroDx()
    {
        return (Math.Abs(manoDx.pos.y - testa.pos.y) < delta && Math.Abs(manoDx.pos.x - testa.pos.x) < delta && manoDx.pos.z > testa.pos.z);
    }

    private bool dietroSx()
    {
        return (Math.Abs(manoSx.pos.y - testa.pos.y) < delta && Math.Abs(manoSx.pos.x - testa.pos.x) < delta && manoSx.pos.z > testa.pos.z);
    }

    private float distanza(KinectWrapper.SkeletonJointPosition elm1, KinectWrapper.SkeletonJointPosition elm2)
    {
        var x = elm1.x - elm2.x;
        var y = elm1.y - elm2.y;
        var z = elm1.z - elm2.z;
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }


}

