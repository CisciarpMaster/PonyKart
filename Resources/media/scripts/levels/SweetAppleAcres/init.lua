local function decreaseKartSpeed(kart, id)
	kart.MaxSpeed = kart.MaxSpeed / 1.4
end

function SweetAppleAcres(level)
	create2DSound("SAA Ambience.ogg", true)
	
	create3DSound("Mountain Stream1.ogg", vector(280.454, 0, -210.054), true)
	create3DSound("Mountain Stream1.ogg", vector(-100.915, 40.8328, -693.665), true)
	
	forEachKart(decreaseKartSpeed)
end