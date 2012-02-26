local function recolor(kart, id)
	-- don't do this for player 0
	if id ~= 0 then
		setMaterial(kart, "TwiCutlass_" .. id)
		setOneSubMaterial(kart.Driver, 0, 0, "TwilightBody_" .. id)
		setOneSubMaterial(kart.Driver, 0, 2, "TwilightHair_" .. id)
	end
end

forEachKart(recolor)