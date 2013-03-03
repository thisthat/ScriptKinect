using System;

namespace AssemblyCSharp
{

    /*
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
        private DateTime newStatus;
        //Serve anche per eseguire e-transazioni verso lo stato di init se non si fa niente per tot tempo 

        private GestureState currentStateGesture = GestureState.Null;
        private Stati state = Stati.Q0;

        //Vecchia posizione dei piedi

        private KinectWrapper.SkeletonJointPosition ps;	//Sinistro
        private KinectWrapper.SkeletonJointPosition pd;	//Destro


        private double _TIMER = 500;

        //UserID
        private uint UserID = 0;

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

        public GestureCamminata(uint _UserID)
            : base()
        {
            this.UserID = _UserID;
            this.ps.x = this.ps.y = this.ps.z = 0;
            this.pd.x = this.pd.y = this.pd.z = 0;
        }

        public GestureState transazione()
        {
            //Info dei point di joint che ci interessano
            KinectWrapper.SkeletonJointTransformation piedeDx;
            KinectWrapper.SkeletonJointTransformation piedeSx;
            KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.RIGHT_FOOT, ref piedeDx);
            KinectWrapper.GetJointTransformation(UserID, KinectWrapper.SkeletonJoint.LEFT_FOOT, ref piedeDx);

            if (state == Stati.Q0)
            {
                //Alzo piede sx
                if (piedeSx.pos.y > this.ps.y && Math.Abs(piedeSx.pos.y - this.ps.y) > 100)
                {
                    this.state = Stati.Q1;
                    newStatus = DateTime.Now;
                    this.ps.y = piedeSx.pos.y;
                }
                //Alzo piede dx
                if (piedeDx.pos.y > this.pd.y && Math.Abs(piedeDx.pos.y - this.pd.y) > 100)
                {
                    this.state = Stati.Q8;
                    newStatus = DateTime.Now;
                    this.pd.y = piedeDx.pos.y;
                }

            }
            else if (state == Stati.Q1)
            {
                //Se il piede sx continua a salire aggiorno le coordinate
                if (piedeSx.pos.y > this.ps.y)
                {
                    this.ps.y = piedeSx.pos.y;
                }
                if (piedeSx.pos.y < this.ps.y && Math.Abs(piedeDx.pos.y - this.ps.y) > 100)
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
                if (piedeDx.pos.y > this.pd.y && Math.Abs(this.pd.y - piedeDx.pos.y) > 100)
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
                if (this.pd.y > piedeDx.pos.y && Math.Abs(this.pd.y - piedeDx.pos.y) > 100)
                {
                    this.state = Stati.Q4;
                    newStatus = DateTime.Now;
                    this.pd.y = piedeDx.pos.y;
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
                if (piedeSx.pos.y > this.ps.y && Math.Abs(piedeSx.pos.y - this.ps.y) > 100)
                {
                    this.state = Stati.Q5;
                    newStatus = DateTime.Now;
                    this.ps.y = piedeSx.pos.y;
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
                if (this.ps.y > piedeSx.pos.y && Math.Abs(piedeSx.pos.y - this.ps.y) > 100)
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
                if (piedeDx.pos.y > this.pd.y && Math.Abs(this.pd.y - piedeDx.pos.y) > 100)
                {
                    this.state = Stati.Q7;
                    newStatus = DateTime.Now;
                    this.pd.y = piedeDx.pos.y;
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
                case Stati.Q5:
                case Stati.Q6:
                case Stati.Q7:
                case Stati.Q11:
                case Stati.Q12:
                case Stati.Q13:
                case Stati.Q14: return GestureState.Camminata;
                default: return GestureState.Null;
            }
        }

        private bool checkTimer()
        {
            return (DateTime.Now.Subtract(newStatus).TotalMilliseconds > this._TIMER);
        }
    }
}

