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

				BrakeForce = 5000,
				MotorForce = 3000,
				TurnAngle = Math.PI / 12f,
				MaxSpeed = 85,

				LatExtremumSlip = 1.0f,
				LatExtremumValue = 0.05f,
				LatAsymptoteSlip = 5.0f,
				LatAsymptoteValue = 0.002f,
				LatStiffnessFactor = 1000000f,

				LongExtremumSlip = 1.0f,
				LongExtremumValue = 0.05f,
				LongAsymptoteSlip = 2.0f,
				LongAsymptoteValue = 0.01f,
				LongStiffnessFactor = 1000000f
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

				BrakeForce = 5000,
				MotorForce = 3000,
				TurnAngle = 0,
				MaxSpeed = 85,

				LatExtremumSlip = 1.0f,
				LatExtremumValue = 0.05f,
				LatAsymptoteSlip = 5.0f,
				LatAsymptoteValue = 0.002f,
				LatStiffnessFactor = 1000000f,

				LongExtremumSlip = 1.0f,
				LongExtremumValue = 0.05f,
				LongAsymptoteSlip = 2.0f,
				LongAsymptoteValue = 0.01f,
				LongStiffnessFactor = 1000000f
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
				MaxSpeed = 85,

				LatExtremumSlip = 0.01f,
				LatExtremumValue = 1,
				LatAsymptoteSlip = 0.04f,
				LatAsymptoteValue = 0.4f,
				LatStiffnessFactor = 4000,

				LongExtremumSlip = 0.01f,
				LongExtremumValue = 1,
				LongAsymptoteSlip = 0.04f,
				LongAsymptoteValue = 0.3f,
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
				MaxSpeed = 85,

				LatExtremumSlip = 0.01f,
				LatExtremumValue = 1,
				LatAsymptoteSlip = 0.04f,
				LatAsymptoteValue = 0.6f,
				LatStiffnessFactor = 5000,

				LongExtremumSlip = 0.01f,
				LongExtremumValue = 1,
				LongAsymptoteSlip = 0.04f,
				LongAsymptoteValue = 0.6f,
				LongStiffnessFactor = 6000,
			};
			wheel.CreateWheelShape(position);
			return wheel;
		}
	}
}
