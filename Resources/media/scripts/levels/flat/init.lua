function flat(level)
	--[[local x, n
	for x = 1, numBgPones do
		spawn("BgPony" .. x, vector(x * 5, 0, 0))
	end ]]
	
	stands = spawn("BgStandsWithPhysics", vector(30, 0, 3.3))
	
	lyra = spawn("BgPony1", vector(35.5, 1.5, 2))
	bonbon = spawn("BgPony2", vector(33, 1.5, 2.6))
	daisy = spawn("BgPony3", vector(16.5, -0.9, 3))
	sparkler = spawn("BgPony4", vector(43, -0.9, 3.5))
	linky = spawn("BgPony5", vector(28, 6, -4))
	carrot = spawn("BgPony6", vector(25, 3, 0))
	rose = spawn("BgPony7", vector(20, 0, 4.5))
	
	lyra:ChangeAnimation("Sit3")
	bonbon:ChangeAnimation("Sit1")
	daisy:ChangeAnimation("Clap")
	sparkler:ChangeAnimation("Cheer1")
	linky:ChangeAnimation("Cheer2")
	carrot:ChangeAnimation("SitCheer1")
	rose:ChangeAnimation("Sit2")
end
