using System;

/* Grafico Automa
digraph {
    rankdir = LR ;
    start [label = "", shape = "plaintext"]
    init [label = "init", shape = "circle"] ;
    sx1 [label = "", shape = "circle"] ;
    sx2 [label = "", shape = "circle"] ;
    sx3 [label = "", shape = "circle"] ;
    end [label = "", shape = "doublecircle"] ;
    dx1 [label = "", shape = "circle"] ;
    dx2 [label = "", shape = "circle"] ;
    dx3 [label = "", shape = "circle"] ;
    start -> init ;
    init -> sx1 [label = "PiedeSx Alto"];
    sx1 -> sx2 [label = "PiedeSx Basso"];
    sx2 -> sx3 [label = "PiedeDx Alto"];
    sx3 -> end [label = "PiedeDx Basso"];
	
    init -> dx1 [label = "PiedeDx Alto"];
    dx1 -> dx2 [label = "PiedeDx Basso"];
    dx2 -> dx3 [label = "PiedeSx Alto"];
    dx3 -> end [label = "PiedeSx Basso"];
	
    end -> init [label = "ε"];

}
    */
public class GestureCamminata : Gesture
{
    //Calcolo il tempo tra init e stato finale, in base alla velocità det se corro o cammino
    private DateTime init;
    private DateTime end;
    private DateTime newStatus; //Serve anche per eseguire e-transazioni verso lo stato di init se non si fa niente per tot tempo 

    //private GestureState currentStateGesture = GestureState.Null;
    private Stati state = Stati.Q0;

    //Vecchia posizione dei piedi
    private KinectWrapper.SkeletonJointPosition ps;	//Sinistro
    private KinectWrapper.SkeletonJointPosition pd;	//Destro

    private KinectWrapper.SkeletonJointPosition ps_init;	//Sinistro di init
    private KinectWrapper.SkeletonJointPosition pd_init;	//Destro di init

    private double _TIMER = 750;
    private int _Delta = 50;
    private GestureState oldGesture = GestureState.Null;

    //Posizioni attuali dei piedi
    KinectWrapper.SkeletonJointTransformation piedeDx = new KinectWrapper.SkeletonJointTransformation();
    KinectWrapper.SkeletonJointTransformation piedeSx = new KinectWrapper.SkeletonJointTransformation();

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

    public GestureCamminata()
        : base()
    {
        this.ps.x = this.ps.y = this.ps.z = 0;
        this.pd.x = this.pd.y = this.pd.z = 0;
    }
    public void Init(KinectWrapper.SkeletonJointTransformation s, KinectWrapper.SkeletonJointTransformation d, uint id)
    {
        this.pd = this.pd_init = d.pos;
        this.ps = this.ps_init = s.pos;
        this.UserID = id;
    }
    public override GestureState transazione()
    {
        //Info dei point di joint che ci interessano

        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_FOOT, ref piedeDx);
        KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_FOOT, ref piedeSx);

        if (state == Stati.Q0)
        {
            ps = ps_init;
            pd = pd_init;
            //Alzo piede sx
            if (this.sinistroSale())
            {
                this.state = Stati.Q1;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
                init = DateTime.Now;
            }
            //Alzo piede dx
            if (this.destroSale())
            {
                this.state = Stati.Q8;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
                init = DateTime.Now;
            }

        }
        else if (state == Stati.Q1)
        {
            //il sx continua a salire
            if (piedeSx.pos.y > this.ps.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q2;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
            }

        }
        else if (this.state == Stati.Q2)
        {
            //Il piede sx continua a scendere
            if (piedeSx.pos.y < this.ps.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //Il piede DX sale
            if (this.destroSale())
            {
                this.state = Stati.Q3;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
            }
        }
        else if (this.state == Stati.Q3)
        {
            //Se il piede dx continua a salire aggiorno le coordinate
            if (piedeDx.pos.y > this.pd.y)
            {
                this.pd.y = piedeDx.pos.y;
            }
            //destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q4;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
                end = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q4)
        {
            //Se il piede dx continua a scendere aggiorno le coordinate
            if (this.pd.y > piedeDx.pos.y)
            {
                this.pd.y = piedeDx.pos.y;
            }
            //Il piede sinistro sale
            if (this.sinistroSale())
            {
                this.state = Stati.Q5;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
                init = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q5)
        {
            //Il sinistro continua salire
            if (piedeSx.pos.y > this.ps.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //Il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q6;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
            }
        }
        else if (this.state == Stati.Q6)
        {
            //Il sinistro continua scendere
            if (this.ps.y > piedeSx.pos.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //Il destro sale
            if (this.destroSale())
            {
                this.state = Stati.Q7;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
            }
        }
        else if (this.state == Stati.Q7)
        {
            ////il dx continua a salire
            if (piedeDx.pos.y > this.pd.y)
            {
                this.pd.y = piedeDx.pos.y;
            }
            //il destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q4;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
                end = DateTime.Now;
            }
        }
        //End primo ramo dell'automa

        else if (this.state == Stati.Q8)
        {
            //il destro continua a salire
            if (piedeDx.pos.y > this.pd.y)
            {
                this.pd.y = piedeDx.pos.y;
            }
            //il destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q9;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
            }
        }

        else if (this.state == Stati.Q9)
        {
            //il destro continua a scendere
            if (this.pd.y > piedeDx.pos.y)
            {
                this.pd.y = piedeDx.pos.y;
            }

            //sinistro sale
            if (this.sinistroSale())
            {
                this.state = Stati.Q10;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
            }
        }

        else if (this.state == Stati.Q10)
        {
            //il sinistro continua a salire
            if (piedeSx.pos.y > this.ps.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //Il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q11;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
                end = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q11)
        {
            //se il sx continua a scendere
            if (this.ps.y > piedeSx.pos.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //se il destro sale
            if (this.destroSale())
            {
                this.state = Stati.Q12;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
                init = DateTime.Now;
            }
        }
        else if (this.state == Stati.Q12)
        {
            //se il destro contiua a salire
            if (piedeDx.pos.y > this.pd.y)
            {
                this.pd.y = piedeDx.pos.y;
            }
            //Il destro scende
            if (this.destroScende())
            {
                this.state = Stati.Q13;
                newStatus = DateTime.Now;
                this.pd.y = piedeDx.pos.y;
            }
        }
        else if (this.state == Stati.Q13)
        {
            //il destro continua a scendere
            if (this.pd.y > piedeDx.pos.y)
            {
                this.pd.y = piedeDx.pos.y;
            }
            //sinistro sale
            if (this.sinistroSale())
            {
                this.state = Stati.Q14;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
            }
        }
        else if (this.state == Stati.Q14)
        {
            //il sinistro continua a salire
            if (piedeSx.pos.y > this.ps.y)
            {
                this.ps.y = piedeSx.pos.y;
            }
            //Il sinistro scende
            if (this.sinistroScende())
            {
                this.state = Stati.Q11;
                newStatus = DateTime.Now;
                this.ps.y = piedeSx.pos.y;
                end = DateTime.Now;
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
            case Stati.Q4:
            case Stati.Q11: return this.oldGesture = (end.Subtract(init).TotalMilliseconds > 750) ? GestureState.Camminata : GestureState.Corsa;

            case Stati.Q5:
            case Stati.Q6:
            case Stati.Q7:
            case Stati.Q12:
            case Stati.Q13:
            case Stati.Q14: return this.oldGesture;
            default: return GestureState.Null;
        }
    }

    private bool checkTimer()
    {
        return (DateTime.Now.Subtract(newStatus).TotalMilliseconds > this._TIMER);
    }

    private bool sinistroSale()
    {
        return (piedeSx.pos.y > this.ps.y && Math.Abs(piedeSx.pos.y - this.ps.y) > this._Delta);
    }

    private bool sinistroScende()
    {
        return (this.ps.y > piedeSx.pos.y && Math.Abs(this.ps.y - piedeSx.pos.y) > this._Delta);
    }
    private bool destroSale()
    {
        return (piedeDx.pos.y > this.pd.y && Math.Abs(piedeDx.pos.y - this.pd.y) > this._Delta);
    }
    private bool destroScende()
    {
        return (this.pd.y > piedeDx.pos.y && Math.Abs(this.pd.y - piedeDx.pos.y) > this._Delta);
    }
}


