local function decreaseKartSpeed(kart, id)
	kart.MaxSpeed = kart.MaxSpeed / 1.4
end

function SweetAppleAcres(level)
	create2DSound("SAA Ambience.ogg", true)
	
	create3DSound("Mountain Stream1.ogg", vector(56.0908, 0, -42.0108), true)
	create3DSound("Mountain Stream1.ogg", vector(-20.1831, 8.16656, -138.733), true)
	
	forEachKart(decreaseKartSpeed)
end