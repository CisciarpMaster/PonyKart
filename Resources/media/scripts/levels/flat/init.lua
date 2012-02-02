--function tr(region, shape, flags)

--end

function flat(level)
	--[[local x, n
	for x = 1, numBgPones do
		spawn("BgPony" .. x, vector(x * 5, 0, 0))
	end ]]
	
	lyra = spawn("BgPony1", vector(5, 3, 0))
	bonbon = spawn("BgPony2", vector(10, 3, 0))
	daisy = spawn("BgPony3", vector(15, 3, 0))

--[[
	local x, z, n
	
	for x = -20, 20 do
		for z = 30, 70 do
			n = math.random(3)
			spawn("StaticAppleTree" .. n, vector(x * 10, 0, z * 10))
		end
	end
	
	for x = -20, 20 do
		for z = -70, -30 do
			n = math.random(3)
			spawn("BillboardAppleTree" .. n, vector(x * 10, 0, z * 10))
		end
	end
	]]
	
	--createBoxTriggerRegion("aTriggerRegion", tr, vec(1000, 1000, 0.5), vec(0, 0, 0), quat(0, 0, 0, 1))
end
