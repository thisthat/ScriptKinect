using System;

public class Gesture4Zampe : Gesture
{
   
    private DateTime newStatus; //Serve anche per eseguire e-transazioni verso lo stato di init se non si fa niente per tot tempo 


    private Stati state = Stati.Q0;

    //Vecchia posizione dlle mani
    private KinectWrapper.SkeletonJointPosition ms;	//Sinistro
    private KinectWrapper.SkeletonJointPosition md;	//Destro

    private KinectWrapper.SkeletonJointPosition ms_init;	//Sinistro di init
    private KinectWrapper.SkeletonJointPosition md_init;	//Destro di init

    private double _TIMER = 750;
    private int _Delta = 50;


    //Posizioni attuali delle mani
    KinectWrapper.SkeletonJointTransformation manoDx = new KinectWrapper.SkeletonJointTransformation();
    KinectWrapper.SkeletonJointTransformation manoSx = new KinectWrapper.SkeletonJointTransformation();

    //User ID
    private uint UserID;

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
        Q9,
        Q10,
        Q11,
        Q12,
        Q13,
        Q14
    }

    public Gesture4Zampe()
        : base()
    {
        this.ms.x = this.ms.y = this.ms.z = 0;
        this.md.x = this.md.y = this.md.z = 0;
    }
    public void Init(KinectWrapper.SkeletonJointTransformation s, KinectWrapper.SkeletonJointTransformation d, uint id)
    {
        this.md = this.md_init = d.pos;
        this.ms = this.ms_init = s.pos;
        this.UserID = id;
    }
    public override GestureState transazione()
    {
        //Info dei point di joint che ci interessano
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_HAND, ref manoDx);
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_HAND, ref manoSx);

        if (state == Stati.Q0)
        {
            ms = ms_init;
            md = md_init;
            //Alzo mano sx
            if (this.sinistroSale())
            {
                this.state = Stati.Q1;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
            //Alzo mano dx
            if (this.destroSale())
            {
                this.state = Stati.Q8;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }

        }
        else if (state == Stati.Q1)
        {
            //il sx continua a salire
            if (manoSx.pos.z > this.ms.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q2;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }

        }
        else if (this.state == Stati.Q2)
        {
            //Il mano sx continua a scendere
            if (manoSx.pos.z < this.ms.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //Il mano DX sale
            if (this.destroSale())
            {
                this.state = Stati.Q3;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }
        else if (this.state == Stati.Q3)
        {
            //Se il mano dx continua a salire aggiorno le coordinate
            if (manoDx.pos.z > this.md.z)
            {
                this.md.z = manoDx.pos.z;
            }
            //destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q4;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }
        else if (this.state == Stati.Q4)
        {
            //Se il mano dx continua a scendere aggiorno le coordinate
            if (this.md.z > manoDx.pos.z)
            {
                this.md.z = manoDx.pos.z;
            }
            //Il mano sinistro sale
            if (this.sinistroSale())
            {
                this.state = Stati.Q5;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
        }
        else if (this.state == Stati.Q5)
        {
            //Il sinistro continua salire
            if (manoSx.pos.z > this.ms.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //Il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q6;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
        }
        else if (this.state == Stati.Q6)
        {
            //Il sinistro continua scendere
            if (this.ms.z > manoSx.pos.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //Il destro sale
            if (this.destroSale())
            {
                this.state = Stati.Q7;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }
        else if (this.state == Stati.Q7)
        {
            ////il dx continua a salire
            if (manoDx.pos.z > this.md.z)
            {
                this.md.z = manoDx.pos.z;
            }
            //il destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q4;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }
        //End primo ramo dell'automa

        else if (this.state == Stati.Q8)
        {
            //il destro continua a salire
            if (manoDx.pos.z > this.md.z)
            {
                this.md.z = manoDx.pos.z;
            }
            //il destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q9;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }

        else if (this.state == Stati.Q9)
        {
            //il destro continua a scendere
            if (this.md.z > manoDx.pos.z)
            {
                this.md.z = manoDx.pos.z;
            }

            //sinistro sale
            if (this.sinistroSale())
            {
                this.state = Stati.Q10;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
        }

        else if (this.state == Stati.Q10)
        {
            //il sinistro continua a salire
            if (manoSx.pos.z > this.ms.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //Il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q11;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
        }
        else if (this.state == Stati.Q11)
        {
            //se il sx continua a scendere
            if (this.ms.z > manoSx.pos.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //se il destro sale
            if (this.destroSale())
            {
                this.state = Stati.Q12;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }
        else if (this.state == Stati.Q12)
        {
            //se il destro contiua a salire
            if (manoDx.pos.z > this.md.z)
            {
                this.md.z = manoDx.pos.z;
            }
            //Il destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q13;
                newStatus = DateTime.Now;
                this.md.z = manoDx.pos.z;
            }
        }
        else if (this.state == Stati.Q13)
        {
            //il destro continua a scendere
            if (this.md.z > manoDx.pos.z)
            {
                this.md.z = manoDx.pos.z;
            }
            //sinistro sale
            if (this.sinistroSale())
            {
                this.state = Stati.Q14;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
        }
        else if (this.state == Stati.Q14)
        {
            //il sinistro continua a salire
            if (manoSx.pos.z > this.ms.z)
            {
                this.ms.z = manoSx.pos.z;
            }
            //Il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q11;
                newStatus = DateTime.Now;
                this.ms.z = manoSx.pos.z;
            }
        }

        //Rilevazione se l'utente è piegato
        KinectWrapper.SkeletonJointTransformation torso = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.TORSO_CENTER, ref torso);
        KinectWrapper.SkeletonJointTransformation testa = new KinectWrapper.SkeletonJointTransformation();
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.HEAD, ref testa);

        var _dz = testa.pos.z - torso.pos.z;
        var _dy = testa.pos.y - torso.pos.y;
        float _angolo = (float)Math.Atan(_dz / _dy);
        _angolo = Math.Abs(_angolo);
        //Controlliamo che l'angolo di piegatura del busto sia tra i valori ammessi
        //Altrimenti resettiamo l'automa allo stato iniziale
        if(_angolo < 0.5 || _angolo > 1){
            this.state = Stati.Q0;
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
            case Stati.Q4:
            case Stati.Q11: 
            case Stati.Q5:
            case Stati.Q6:
            case Stati.Q7:
            case Stati.Q12:
            case Stati.Q13:
            case Stati.Q14: return GestureState.Gattoni;
            default: return GestureState.Null;
        }
    }

    private bool checkTimer()
    {
        return (DateTime.Now.Subtract(newStatus).TotalMilliseconds > this._TIMER);
    }

    private bool sinistroSale()
    {
        return (manoSx.pos.z > this.ms.z && Math.Abs(manoSx.pos.z - this.ms.z) > this._Delta);
    }

    private bool sinistroScende()
    {
        return (this.ms.z > manoSx.pos.z && Math.Abs(this.ms.z - manoSx.pos.z) > this._Delta);
    }
    private bool destroSale()
    {
        return (manoDx.pos.z > this.md.z && Math.Abs(manoDx.pos.z - this.md.z) > this._Delta);
    }
    private bool destroScende()
    {
        return (this.md.z > manoDx.pos.z && Math.Abs(this.md.z - manoDx.pos.z) > this._Delta);
    }
}


