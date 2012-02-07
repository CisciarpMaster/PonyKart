function flat(level)
	--[[local x, n
	for x = 1, numBgPones do
		spawn("BgPony" .. x, vector(x * 5, 0, 0))
	end ]]
	
	stands = spawn("BgStandsWithPhysics", vector(30, 0, 3.3))
	
	lyra = spawn("BgPonyLyra", vector(35.5, 1.5, 2))
	bonbon = spawn("BgPonyBonBon", vector(33, 1.5, 2.6))
	daisy = spawn("BgPonyDaisy", vector(16.5, -0.9, 3))
	sparkler = spawn("BgPonySparkler", vector(43, -0.9, 3.5))
	linky = spawn("BgPonyLinky", vector(28, 6, -4))
	carrot = spawn("BgPonyCarrot", vector(25, 3, 0))
	rose = spawn("BgPonyRose", vector(20, 0, 4.5))
	diamond = spawn("BgPonyDiamond", vector(45.5, -0.9, 4.5))
	lily = spawn("BgPonyLily", vector(13, -0.9, 3.5))
	cloudkicker = spawn("BgPonyCloudKicker", vector(10, 2, 0))
	flora = spawn("BgPonyFlora", vector(7, -0.9, -3.5))
	
	lyra:Sit()
	bonbon:Sit()
	daisy:Stand()
	daisy:Cheer()
	sparkler:Stand()
	sparkler:Cheer()
	linky:ChangeAnimation("Cheer2")
	carrot:Sit()
	carrot:Cheer()
	rose:Sit()
	diamond:Stand()
	lily:Stand()
	cloudkicker:Fly()
	flora:Stand()
end
