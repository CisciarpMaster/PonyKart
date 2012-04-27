
function WhitetailWoods(level)
	derpy = spawnDerpy(vector(0, 0, 0))
	derpy:ChangeAnimation("Hover1")
	derpy:AttachToKart(vector(-1, 1, 2), playerKart())
end

--[[
function increaseKartSpeed(kart, id)
	kart.MaxSpeed = kart.MaxSpeed * 1.4
end

forEachKart(increaseKartSpeed)
]]