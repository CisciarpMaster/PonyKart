function WhitetailWoods(level)

end

function increaseKartSpeed(kart, id)
	kart.MaxSpeed = kart.MaxSpeed * 1.75
end

forEachKart(increaseKartSpeed)