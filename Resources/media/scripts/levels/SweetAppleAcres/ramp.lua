local function rampIncSpeed(r, body, flags, i)
	local kart = getKartFromBody(body)
	if kart ~= nil and isEnterFlag(flags) then
		-- for regular speed
		--kart.MaxSpeed = kart.InitialMaxSpeed * 1.4
		-- for SAA's slow speed
		kart.MaxSpeed = kart.InitialMaxSpeed
	end
end

local function rampDecSpeed(r, body, flags, i)
	local kart = getKartFromBody(body)
	if kart ~= nil and isEnterFlag(flags) then
		-- for regular speed
		--kart.MaxSpeed = kart.InitialMaxSpeed
		-- for SAA's slow speed
		kart.MaxSpeed = kart.InitialMaxSpeed / 1.4
	end
end

hookFunctionToTriggerRegion("AITriggerRegion25", rampIncSpeed)
hookFunctionToTriggerRegion("AITriggerRegion26", rampDecSpeed)