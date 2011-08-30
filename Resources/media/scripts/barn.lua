-- load up our types
luanet.load_assembly("BulletSharp")
HingeConstraint = luanet.import_type("BulletSharp.HingeConstraint")

function barn(lthing)
	unit_y = vector(0, 1, 0)

	-- these two need to be rotated the other way around
	topleft = spawn("Barn_Top_Doors", addVectors(lthing.SpawnPosition, vector(-2.95, 7.4, 16.8)))
	setBodyOrientation(topleft.Body, quaternion(0, 1, 0, 0))
	bottomleft = spawn("Barn_Bottom_Doors", addVectors(lthing.SpawnPosition, vector(-2.95, 1.9, 16.8)))
	setBodyOrientation(bottomleft.Body, quaternion(0, 1, 0, 0))
	
	-- these ones are fine though
	topright = spawn("Barn_Top_Doors", addVectors(lthing.SpawnPosition, vector(4.836, 7.4, 16.8)))
	bottomright = spawn("Barn_Bottom_Doors", addVectors(lthing.SpawnPosition, vector(4.846, 1.9, 16.8)))
	
	
	-- make hinges
	TLhinge = HingeConstraint(topleft.Body, lthing.Body, vector(3.75, 0, 0), vector(-6.8, 7.4, 16.929), unit_y, unit_y)
	BLhinge = HingeConstraint(bottomleft.Body, lthing.Body, vector(3.75, 0, 0), vector(-6.8, 1.9, 16.929), unit_y, unit_y)
	
	TRhinge = HingeConstraint(topright.Body, lthing.Body, vector(3.75, 0, 0), vector(8.685, 7.4, 16.929), unit_y, unit_y)
	BRhinge = HingeConstraint(bottomright.Body, lthing.Body, vector(3.75, 0, 0), vector(8.685, 1.9, 16.929), unit_y, unit_y)
	
	
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
	topleft.Body.AngularVelocity = vector(0, 0, 0)
	topleft.Body.LinearVelocity = vector(0, 0, 0)
	bottomleft.Body.AngularVelocity = vector(0, 0, 0)
	bottomleft.Body.LinearVelocity = vector(0, 0, 0)
	topright.Body.AngularVelocity = vector(0, 0, 0)
	bottomleft.Body.LinearVelocity = vector(0, 0, 0)
	bottomright.Body.AngularVelocity = vector(0, 0, 0)
	bottomleft.Body.LinearVelocity = vector(0, 0, 0)
end