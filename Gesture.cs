using System;

namespace AssemblyCSharp
{
	public abstract class Gesture
	{
		public enum GestureState {
			Null = 0,
			Seduto = 1,
			Piedi = 2,
			Camminata = 4,
			Corsa = 8,
			Indietro = 16,
			Gattoni = 32,
			Salto = 64,
			SpostamentoSX = 128,
			SpostamentoDX = 256,
			Maschera = 515,
			PosizioneSicurezza = 1024,
			Cintura = 2048
		};
		
		public abstract GestureState transazione(KinectWrapper.SkeletonJointTransformation sk);
	}
}

