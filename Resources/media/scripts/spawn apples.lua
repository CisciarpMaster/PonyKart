local appleRadius = 0.7

-- an apple's radius is 0.75
function spawnApples(numberOfApples, position)
local a
	for a = 0, numberOfApples do
		vspawn("Apple", addVectors(position, vector(0, a * appleRadius, 0)))
	end
end

-- we spawn stuff from the origin of the lthing, plus a little offset if we want.
-- This will use positions relative to the lthing, taking rotation and stuff into mind if we need it.
-- Width is X, height is Y, length is Z.
function spawnMoreApples(lthing, offsetX, offsetY, offsetZ, width, height, length)
	local offset = vector(offsetX, offsetY, offsetZ)
	local x, y, z, mx, my, mz
	
	for x = 0, width - 1 do
		-- multiply these by the radius of the apple
		mx = x * appleRadius
		
		for y = 0, height - 1 do
			my = y * appleRadius
			
			for z = 0, length - 1 do
				mz = z * appleRadius
				
				relativeSpawn("Apple", lthing, addVectors(offset, vector(mx, my, mz)))
			end
		end
	end
end