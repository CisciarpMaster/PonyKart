local function recolor(kart, id)
	-- don't do this for player 0
	if id ~= 0 and kart.Player.Character ~= "Applejack" and kart.Player.Character ~= "Rainbow Dash" then
		setMaterial(kart, "TwiCutlass_" .. id)
		--[[if kart.Player.Character == "Twilight Sparkle" then
			setOneSubMaterial(kart.Driver, 0, 0, "TwilightBody_" .. id)
			setOneSubMaterial(kart.Driver, 0, 2, "TwilightHair_" .. id)
		end]]
	end
end

forEachKart(recolor)