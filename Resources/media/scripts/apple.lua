-- we have four colors of apples: two meshes, two materials, and two textures.
-- one mesh uses the top half of the texture, the other uses the bottom half.
-- one material is for the red texture, the other is for the green one.
-- This is basically a randomiser that picks one mesh and one material out of the options available and creates one of the 4 colors of apple.
function apple(lthing)
	num = math.random(4)
	local name
	local mat
	
	if num > 2 then
		name = "Apple1"
	else
		name = "Apple2"
	end
	
	if num == 1 or num == 3 then
		mat = "RedApple"
	else
		mat = "GreenApple"
	end
	
	t = spawn(name, lthing.SpawnPosition)
	setMaterial(t, mat)
	deactivateThing(t)
	
	lthing:Dispose()
end