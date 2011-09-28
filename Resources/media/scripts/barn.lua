-- load up our types
luanet.load_assembly("BulletSharp")
HingeConstraint = luanet.import_type("BulletSharp.HingeConstraint")

function barn(lthing)
	local unit_y = vector(0, 1, 0)

	-- these two need to be rotated the other way around
	local topleft = spawn("Barn_Top_Doors", addVectors(lthing.SpawnPosition, vector(-2.95, 7.4, 16.8)))
	setBodyOrientation(topleft.Body, quaternion(0, 1, 0, 0))
	local bottomleft = spawn("Barn_Bottom_Doors", addVectors(lthing.SpawnPosition, vector(-2.95, 1.9, 16.8)))
	setBodyOrientation(bottomleft.Body, quaternion(0, 1, 0, 0))
	
	-- these ones are fine though
	local topright = spawn("Barn_Top_Doors", addVectors(lthing.SpawnPosition, vector(4.836, 7.4, 16.8)))
	local bottomright = spawn("Barn_Bottom_Doors", addVectors(lthing.SpawnPosition, vector(4.846, 1.9, 16.8)))
	
	
	-- make hinges
	local TLhinge = HingeConstraint(topleft.Body, lthing.Body, vector(3.75, 0, 0), vector(-6.8, 7.4, 16.929), unit_y, unit_y)
	local BLhinge = HingeConstraint(bottomleft.Body, lthing.Body, vector(3.75, 0, 0), vector(-6.8, 1.9, 16.929), unit_y, unit_y)
	
	local TRhinge = HingeConstraint(topright.Body, lthing.Body, vector(3.75, 0, 0), vector(8.685, 7.4, 16.929), unit_y, unit_y)
	local BRhinge = HingeConstraint(bottomright.Body, lthing.Body, vector(3.75, 0, 0), vector(8.685, 1.9, 16.929), unit_y, unit_y)
	
	
	-- remember that methods use : and not .!
	TLhinge:SetLimit(1.57, 3.14)
	BLhinge:SetLimit(1.57, 3.14)
	TRhinge:SetLimit(0, 1.57)
	BRhinge:SetLimit(0, 1.57)
	-- and then add our constraints to the world
	addConstraint(TLhinge, false)
	addConstraint(BLhinge, false)
	addConstraint(TRhinge, false)
	addConstraint(BRhinge, false)
	-- make them stop moving so they're always completely closed
	local vecZero = vector(0, 0, 0)
	topleft.Body.AngularVelocity = vecZero
	topleft.Body.LinearVelocity = vecZero
	bottomleft.Body.AngularVelocity = vecZero
	bottomleft.Body.LinearVelocity = vecZero
	topright.Body.AngularVelocity = vecZero
	bottomleft.Body.LinearVelocity = vecZero
	bottomright.Body.AngularVelocity = vecZero
	bottomleft.Body.LinearVelocity = vecZero
end