using Mogre;

namespace Ponykart.Actors {
	public abstract class WheelFactory {

		// lat = sideways grip, long = forwards grip
		//										1.0f				 0.02f						2.0f					   0.01f						1000000f
		public static float LatExtremumSlip = 1.0f, LatExtremumValue = 0.05f, LatAsymptoteSlip = 5.0f, LatAsymptoteValue = 0.002f, LatStiffnessFactor = 1000000f,
							LongExtremumSlip = 1.0f, LongExtremumValue = 0.05f, LongAsymptoteSlip = 2.0f, LongAsymptoteValue = 0.01f, LongStiffnessFactor = 1000000f;


		public static Wheel CreateFrontWheel(Kart owner, Vector3 position) {
			Wheel wheel = new Wheel(owner, position) {
				Radius = 0.5f,
				Suspension = 0.5f,
				SpringRestitution = 7000,
				SpringDamping = 800,
				SpringBias = 0,

				BrakeForce = 4000,
				MotorForce = 3000,
				TurnAngle = Math.PI / 20f,
				// the maximum axle speed a kart with these wheels reaches is only about 87ish anyway (40 linear vel)
				MaxSpeed = 100,

				LatExtremumSlip = 1.0f,
				LatExtremumValue = 1f,
				LatAsymptoteSlip = 3.0f,
				LatAsymptoteValue = 0.2f,
				LatStiffnessFactor = 40000f,

				LongExtremumSlip = 1.0f,
				LongExtremumValue = 1f,
				LongAsymptoteSlip = 2.0f,
				LongAsymptoteValue = 0.7f,
				LongStiffnessFactor = 50000f
			};
			wheel.CreateWheelShape(position);
			return wheel;
		}

		public static Wheel CreateBackWheel(Kart owner, Vector3 position) {
			Wheel wheel = new Wheel(owner, position) {
				Radius = 0.5f,
				Suspension = 0.5f,
				SpringRestitution = 7000,
				SpringDamping = 800,
				SpringBias = 0,

				BrakeForce = 4000,
				MotorForce = 3000,
				TurnAngle = 0,
				MaxSpeed = 100,

				LatExtremumSlip = 1.0f,
				LatExtremumValue = 1f,
				LatAsymptoteSlip = 3.0f,
				LatAsymptoteValue = 0.2f,
				LatStiffnessFactor = 40000f,

				LongExtremumSlip = 1.0f,
				LongExtremumValue = 1f,
				LongAsymptoteSlip = 2.0f,
				LongAsymptoteValue = 0.7f,
				LongStiffnessFactor = 50000f
			};
			wheel.CreateWheelShape(position);
			return wheel;
		}

		// top speed with these two seems to be about 100, though anywhere past 80 is just a slow increase
		public static Wheel CreateAltFrontWheel(Kart owner, Vector3 position) {
			Wheel wheel = new Wheel(owner, position) {
				Radius = 0.5f,
				Suspension = 0.5f,
				SpringRestitution = 7000,
				SpringDamping = 800,
				SpringBias = 0,

				BrakeForce = 5000,
				MotorForce = 4000,
				TurnAngle = Math.PI / 10f,
				MaxSpeed = 100,

				LatExtremumSlip = 0.01f,
				LatExtremumValue = 1,
				LatAsymptoteSlip = 0.04f,
				LatAsymptoteValue = 0.8f,
				LatStiffnessFactor = 4000,

				LongExtremumSlip = 0.01f,
				LongExtremumValue = 1,
				LongAsymptoteSlip = 0.04f,
				LongAsymptoteValue = 0.8f,
				LongStiffnessFactor = 5000,
			};
			wheel.CreateWheelShape(position);
			return wheel;
		}


		public static Wheel CreateAltBackWheel(Kart owner, Vector3 position) {
			Wheel wheel = new Wheel(owner, position) {
				Radius = 0.5f,
				Suspension = 0.5f,
				SpringRestitution = 7000,
				SpringDamping = 800,
				SpringBias = 0,

				BrakeForce = 5000,
				MotorForce = 4000,
				TurnAngle = 0,
				MaxSpeed = 100,

				LatExtremumSlip = 0.01f,
				LatExtremumValue = 1,
				LatAsymptoteSlip = 0.04f,
				LatAsymptoteValue = 0.8f,
				LatStiffnessFactor = 4000,

				LongExtremumSlip = 0.01f,
				LongExtremumValue = 1,
				LongAsymptoteSlip = 0.04f,
				LongAsymptoteValue = 0.8f,
				LongStiffnessFactor = 6000,
			};
			wheel.CreateWheelShape(position);
			return wheel;
		}
	}
}
