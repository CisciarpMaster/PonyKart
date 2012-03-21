--[[
local threshold = 2

local regions = {}

-- if it's a kart with a computer player, this tells it the next waypoint to go to
local function goTo(currentRegion, nextRegion, body, info)
	local k = getKartFromBody(body)
	if k ~= nil then
		if k.Player.IsComputerControlled then
			k.Player:CalculateNewWaypoint(currentRegion, nextRegion, info)
		end
	end
end

local iden = quat(0, 0, 0, 1)

--
	 how to make trigger regions:
	 1) make a box in 3ds max, 2x2x2
	 2) align to world, so when you set the position gizmo, "forward" is -X
	 3) place them all around the level


regions[0] = createBoxTriggerRegion("TriggerRegion0", (function(r,b,f,i) goTo(r,regions[1],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-1.54619, 7.12765, 64.91), iden)
regions[1] = createBoxTriggerRegion("TriggerRegion1", (function(r,b,f,i) goTo(r,regions[2],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-10.0019, 6.65829, 64.7552), iden)
regions[2] = createBoxTriggerRegion("TriggerRegion2", (function(r,b,f,i) goTo(r,regions[3],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-30.3447, 7.57517, 61.2675), iden)
regions[3] = createBoxTriggerRegion("TriggerRegion3", (function(r,b,f,i) goTo(r,regions[4],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-50.2704, 12.1273, 66.4486), iden)
regions[4] = createBoxTriggerRegion("TriggerRegion4", (function(r,b,f,i) goTo(r,regions[5],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-61.1527, 12.1273, 64.9944), quat(0, -0.182236, 7.96577e-9, 0.983255))
regions[5] = createBoxTriggerRegion("TriggerRegion5", (function(r,b,f,i) goTo(r,regions[6],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-66.6559, 12.1273, 61.3626), quat(0, -0.413582, 1.80783e-8, 0.910467))
regions[6] = createBoxTriggerRegion("TriggerRegion6", (function(r,b,f,i) goTo(r,regions[7],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-68.8979, 12.1273, 54.9452), quat(0, -0.707107, 3.09086e-8, 0.707107))
regions[7] = createBoxTriggerRegion("TriggerRegion7", (function(r,b,f,i) goTo(r,regions[8],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-66.4357, 12.1273, 34.7167), quat(0, -0.707107, 3.09086e-8, 0.707107))
regions[8] = createBoxTriggerRegion("TriggerRegion8", (function(r,b,f,i) goTo(r,regions[9],b,i) end), vec(threshold, 9.08414, 6.12005), vec(-66.0849, 12.1273, 15.7917), quat(0, -0.793493, 3.46847e-8, 0.608579))
regions[9] = createBoxTriggerRegion("TriggerRegion9", (function(r,b,f,i) goTo(r,regions[10],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-58.5073, 12.1273, -2.43629), quat(0, -0.793493, 3.46847e-8, 0.608579))
regions[10] = createBoxTriggerRegion("TriggerRegion10", (function(r,b,f,i) goTo(r,regions[11],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-57.5766, 12.1273, -11.9797), quat(0, -0.686651, 3.00145e-8, 0.726987))
regions[11] = createBoxTriggerRegion("TriggerRegion11", (function(r,b,f,i) goTo(r,regions[12],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-57.8779, 12.1273, -23.5756), quat(0, -0.702055, 3.06878e-8, 0.712123))
regions[12] = createBoxTriggerRegion("TriggerRegion12", (function(r,b,f,i) goTo(r,regions[13],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-70.2565, 6.60326, -39.7499), quat(0, -0.517031, 2.26002e-8, 0.855967))
regions[13] = createBoxTriggerRegion("TriggerRegion13", (function(r,b,f,i) goTo(r,regions[14],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-59.6374, 10.2226, -31.3355), quat(0, -0.457171, 1.99836e-8, 0.889379))
regions[14] = createBoxTriggerRegion("TriggerRegion14", (function(r,b,f,i) goTo(r,regions[15],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-71.3329, 6.60326, -45.3571), quat(0, -0.706154, 3.0867e-8, 0.708058))
regions[15] = createBoxTriggerRegion("TriggerRegion15", (function(r,b,f,i) goTo(r,regions[16],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-70.0044, 6.60326, -55.34), quat(0, -0.783831, 3.42623e-8, 0.620974))
regions[16] = createBoxTriggerRegion("TriggerRegion16", (function(r,b,f,i) goTo(r,regions[17],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-68.0378, 6.60326, -68.1238), quat(0, -0.709306, 3.10048e-8, 0.7049))
regions[17] = createBoxTriggerRegion("TriggerRegion17", (function(r,b,f,i) goTo(r,regions[18],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-69.4618, 6.60326, -80.6094), quat(0, -0.652938, 2.85408e-8, 0.757412))
regions[18] = createBoxTriggerRegion("TriggerRegion18", (function(r,b,f,i) goTo(r,regions[19],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-71.1823, 9.9512, -94.3763), quat(0, -0.652938, 2.85408e-8, 0.757412))
regions[19] = createBoxTriggerRegion("TriggerRegion19", (function(r,b,f,i) goTo(r,regions[20],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-72.5592, 15.2764, -107.206), quat(0, -0.690169, 3.01683e-8, 0.723648))
regions[20] = createBoxTriggerRegion("TriggerRegion20", (function(r,b,f,i) goTo(r,regions[21],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-72.7342, 15.2764, -125.554), quat(0, -0.708967, 3.09899e-8, 0.705242))
regions[21] = createBoxTriggerRegion("TriggerRegion21", (function(r,b,f,i) goTo(r,regions[22],b,i) end), vec(threshold, 9.08414, 6.02873), vec(-70.3522, 15.2764, -133.937), quat(0, 0.887233, -3.87822e-8, -0.461322))
regions[22] = createBoxTriggerRegion("TriggerRegion22", (function(r,b,f,i) goTo(r,regions[23],b,i) end), vec(threshold, 9.08414, 5.77025), vec(-65.2835, 15.2764, -137.743), quat(0, 0.98489, -4.30509e-8, -0.173182))
regions[23] = createBoxTriggerRegion("TriggerRegion23", (function(r,b,f,i) goTo(r,regions[24],b,i) end), vec(threshold, 9.08414, 6.01229), vec(-52.3667, 15.2764, -138.091), quat(0, 0.999994, -4.37111e-8, -0.00337965))
regions[24] = createBoxTriggerRegion("TriggerRegion24", (function(r,b,f,i) goTo(r,regions[25],b,i) end), vec(threshold, 9.08414, 5.23195), vec(-36.9569, 15.2764, -138.295), quat(0, 0.999994, -4.37111e-8, -0.00337965))
regions[25] = createBoxTriggerRegion("TriggerRegion25", (function(r,b,f,i) goTo(r,regions[26],b,i) end), vec(threshold, 9.08414, 3.57393), vec(-24.8876, 15.2764, -138.416), quat(0, 0.999994, -4.37111e-8, -0.00337965))
regions[26] = createBoxTriggerRegion("TriggerRegion26", (function(r,b,f,i) goTo(r,regions[27],b,i) end), vec(threshold, 9.08414, 6.25344), vec(1.54176, 15.2764, -138.911), quat(0, 0.999994, -4.37111e-8, -0.00337965))
regions[27] = createBoxTriggerRegion("TriggerRegion27", (function(r,b,f,i) goTo(r,regions[28],b,i) end), vec(threshold, 9.08414, 6.08911), vec(23.8561, 15.2764, -138.707), quat(0, 0.999233, -4.36779e-8, 0.0391474))
regions[28] = createBoxTriggerRegion("TriggerRegion28", (function(r,b,f,i) goTo(r,regions[29],b,i) end), vec(threshold, 9.08414, 6.08911), vec(36.1654, 15.2764, -136.496), quat(0, 0.988213, -4.31962e-8, 0.153084))
regions[29] = createBoxTriggerRegion("TriggerRegion29", (function(r,b,f,i) goTo(r,regions[30],b,i) end), vec(threshold, 9.08414, 5.98463), vec(54.4567, 17.3674, -128.08), quat(0, 0.982075, -4.29279e-8, 0.188488))
regions[30] = createBoxTriggerRegion("TriggerRegion30", (function(r,b,f,i) goTo(r,regions[31],b,i) end), vec(threshold, 10.3815, 5.98463), vec(70.3102, 16.2928, -124.221), quat(0, 0.995419, -4.35111e-8, 0.0956121))
regions[31] = createBoxTriggerRegion("TriggerRegion31", (function(r,b,f,i) goTo(r,regions[32],b,i) end), vec(threshold, 11.0518, 5.98463), vec(76.1114, 15.2764, -122.268), quat(0, 0.962053, -4.20527e-8, 0.272861))
regions[32] = createBoxTriggerRegion("TriggerRegion32", (function(r,b,f,i) goTo(r,regions[33],b,i) end), vec(threshold, 11.0518, 5.98463), vec(79.5891, 15.2764, -117.689), quat(0, 0.832278, -3.638e-8, 0.554358))
regions[33] = createBoxTriggerRegion("TriggerRegion33", (function(r,b,f,i) goTo(r,regions[34],b,i) end), vec(threshold, 11.0518, 5.98463), vec(81.1237, 14.5181, -109.876), quat(0, 0.688004, -3.00736e-8, 0.725707))
regions[34] = createBoxTriggerRegion("TriggerRegion34", (function(r,b,f,i) goTo(r,regions[35],b,i) end), vec(threshold, 11.0518, 5.98463), vec(78.8495, 14.261, -104.611), quat(0, 0.416351, -1.81993e-8, 0.909204))
regions[35] = createBoxTriggerRegion("TriggerRegion35", (function(r,b,f,i) goTo(r,regions[36],b,i) end), vec(threshold, 11.0518, 5.98463), vec(74.5967, 14.3597, -101.37), quat(0, 0.264607, -1.15663e-8, 0.964356))
regions[36] = createBoxTriggerRegion("TriggerRegion36", (function(r,b,f,i) goTo(r,regions[37],b,i) end), vec(threshold, 11.0518, 5.98463), vec(64.071, 13.856, -95.0144), quat(0, 0.307482, -1.34405e-8, 0.951554))
regions[37] = createBoxTriggerRegion("TriggerRegion37", (function(r,b,f,i) goTo(r,regions[38],b,i) end), vec(threshold, 9.57837, 5.98463), vec(58.7673, 11.8562, -89.4317), quat(0, 0.490536, -2.1442e-8, 0.871421))
regions[38] = createBoxTriggerRegion("TriggerRegion38", (function(r,b,f,i) goTo(r,regions[39],b,i) end), vec(threshold, 7.03141, 5.98463), vec(56.9424, 9.11584, -84.0249), quat(0, 0.647676, -2.83108e-8, 0.761916))
regions[39] = createBoxTriggerRegion("TriggerRegion39", (function(r,b,f,i) goTo(r,regions[40],b,i) end), vec(threshold, 7.66983, 5.98463), vec(56.4566, 9.54114, -78.4622), quat(0, 0.69686, -3.04607e-8, 0.717207))
regions[40] = createBoxTriggerRegion("TriggerRegion40", (function(r,b,f,i) goTo(r,regions[41],b,i) end), vec(threshold, 7.53426, 5.98463), vec(56.5304, 8.15878, -61.1949), quat(0, 0.708168, -3.0955e-8, 0.706044))
regions[41] = createBoxTriggerRegion("TriggerRegion41", (function(r,b,f,i) goTo(r,regions[42],b,i) end), vec(threshold, 7.26522, 3.98554), vec(56.0566, 7.13697, -50.2763), quat(0, 0.708168, -3.0955e-8, 0.706044))
regions[42] = createBoxTriggerRegion("TriggerRegion42", (function(r,b,f,i) goTo(r,regions[43],b,i) end), vec(threshold, 8.3275, 3.8308), vec(56.0984, 7.9123, -34.7473), quat(0, 0.708168, -3.0955e-8, 0.706044))
regions[43] = createBoxTriggerRegion("TriggerRegion43", (function(r,b,f,i) goTo(r,regions[44],b,i) end), vec(threshold, 9.17875, 5.96003), vec(57.4732, 9.9217, -19.38), quat(0, 0.708168, -3.0955e-8, 0.706044))
regions[44] = createBoxTriggerRegion("TriggerRegion44", (function(r,b,f,i) goTo(r,regions[45],b,i) end), vec(threshold, 9.17875, 5.96003), vec(55.6529, 9.9217, -11.3846), quat(0, 0.529963, -2.31654e-8, 0.848021))
regions[45] = createBoxTriggerRegion("TriggerRegion45", (function(r,b,f,i) goTo(r,regions[46],b,i) end), vec(threshold, 9.17875, 5.96003), vec(50.9605, 9.9217, -5.88872), quat(0, 0.34201, -1.49497e-8, 0.939696))
regions[46] = createBoxTriggerRegion("TriggerRegion46", (function(r,b,f,i) goTo(r,regions[47],b,i) end), vec(threshold, 9.17875, 5.96003), vec(45.4982, 9.17943, -3.03406), quat(0, 0.0689314, -3.01309e-9, 0.997621))
regions[47] = createBoxTriggerRegion("TriggerRegion47", (function(r,b,f,i) goTo(r,regions[48],b,i) end), vec(threshold, 9.17875, 5.96003), vec(39.7764, 8.48086, -4.21007), quat(0, -0.223315, 9.76139e-9, 0.974746))
regions[48] = createBoxTriggerRegion("TriggerRegion48", (function(r,b,f,i) goTo(r,regions[49],b,i) end), vec(threshold, 9.17875, 5.96003), vec(34.202, 7.95923, -8.56906), quat(0, -0.339853, 1.48554e-8, 0.940479))
regions[49] = createBoxTriggerRegion("TriggerRegion49", (function(r,b,f,i) goTo(r,regions[50],b,i) end), vec(threshold, 9.17875, 5.96003), vec(24.2669, 7.53371, -15.9736), quat(0, -0.28378, 1.24044e-8, 0.958889))
regions[50] = createBoxTriggerRegion("TriggerRegion50", (function(r,b,f,i) goTo(r,regions[51],b,i) end), vec(threshold, 10.0531, 5.96003), vec(15.7566, 8.31882, -19.74), quat(0, -0.0918454, 4.01469e-9, 0.995773))
regions[51] = createBoxTriggerRegion("TriggerRegion51", (function(r,b,f,i) goTo(r,regions[52],b,i) end), vec(threshold, 9.77881, 5.96003), vec(10.5107, 8.40353, -19.4732), quat(0, 0.156355, -6.83449e-9, 0.987701))
regions[52] = createBoxTriggerRegion("TriggerRegion52", (function(r,b,f,i) goTo(r,regions[53],b,i) end), vec(threshold, 9.72146, 5.96003), vec(6.10852, 8.02191, -16.6711), quat(0, 0.388532, -1.69833e-8, 0.921435))
regions[53] = createBoxTriggerRegion("TriggerRegion53", (function(r,b,f,i) goTo(r,regions[54],b,i) end), vec(threshold, 9.85302, 5.96003), vec(2.002, 8.5309, -10.2026), quat(0, 0.542792, -2.37262e-8, 0.839867))
regions[54] = createBoxTriggerRegion("TriggerRegion54", (function(r,b,f,i) goTo(r,regions[55],b,i) end), vec(threshold, 9.85528, 5.96003), vec(1.15998, 7.8463, -5.8697), quat(0, 0.706318, -3.08741e-8, 0.707895))
regions[55] = createBoxTriggerRegion("TriggerRegion55", (function(r,b,f,i) goTo(r,regions[56],b,i) end), vec(threshold, 9.87793, 5.96003), vec(2.37008, 8.39083, -1.43587), quat(0, 0.843891, -3.68876e-8, 0.536515))
regions[56] = createBoxTriggerRegion("TriggerRegion56", (function(r,b,f,i) goTo(r,regions[57],b,i) end), vec(threshold, 9.93936, 5.96003), vec(7.87017, 8.21101, 6.23532), quat(0, 0.891723, -3.89785e-8, 0.452581))
regions[57] = createBoxTriggerRegion("TriggerRegion57", (function(r,b,f,i) goTo(r,regions[58],b,i) end), vec(threshold, 9.17875, 5.96419), vec(16.6602, 7.43882, 22.071), quat(0, 0.761928, -3.33049e-8, 0.647662))
regions[58] = createBoxTriggerRegion("TriggerRegion58", (function(r,b,f,i) goTo(r,regions[59],b,i) end), vec(threshold, 9.17875, 2.9166), vec(16.5766, 7.43882, 24.6348), quat(0, 0.789949, -3.45297e-8, 0.613173))
regions[59] = createBoxTriggerRegion("TriggerRegion59", (function(r,b,f,i) goTo(r,regions[60],b,i) end), vec(threshold, 9.17875, 1.8863), vec(18.0418, 7.43882, 31.9352), quat(0, 0.736594, -3.21975e-8, 0.676335))
regions[60] = createBoxTriggerRegion("TriggerRegion60", (function(r,b,f,i) goTo(r,regions[61],b,i) end), vec(threshold, 9.17875, 1.19564), vec(18.5824, 7.43882, 43.4826), quat(0, 0.709603, -3.10177e-8, 0.704602))
regions[61] = createBoxTriggerRegion("TriggerRegion61", (function(r,b,f,i) goTo(r,regions[62],b,i) end), vec(threshold, 9.17875, 1.50332), vec(18.5322, 7.43882, 49.563), quat(0, 0.709603, -3.10177e-8, 0.704602))
regions[62] = createBoxTriggerRegion("TriggerRegion62", (function(r,b,f,i) goTo(r,regions[63],b,i) end), vec(threshold, 9.17875, 2.56893), vec(18.2251, 7.43882, 53.4514), quat(0, 0.656964, -2.87168e-8, 0.753922))
regions[63] = createBoxTriggerRegion("TriggerRegion63", (function(r,b,f,i) goTo(r,regions[64],b,i) end), vec(threshold, 9.17875, 3.44744), vec(17.2011, 7.43882, 56.8358), quat(0, 0.430834, -1.88324e-8, 0.902431))
regions[64] = createBoxTriggerRegion("TriggerRegion64", (function(r,b,f,i) goTo(r,regions[65],b,i) end), vec(threshold, 9.17875, 5.55435), vec(17.2302, 7.43882, 59.9302), quat(0, 0.389128, -1.70093e-8, 0.921184))
regions[65] = createBoxTriggerRegion("TriggerRegion65", (function(r,b,f,i) goTo(r,regions[0],b,i) end), vec(threshold, 9.17875, 6.09349), vec(9.06777, 7.43882, 64.3175), quat(0, 0.0713328, -3.11805e-9, 0.997453))
--regions[] = createBoxTriggerRegion("TriggerRegion", (function(r,b,f,i) goTo(r,regions[],b,i) end), vec(threshold, ), vec(), iden)
--regions[] = createBoxTriggerRegion("TriggerRegion", (function(r,b,f,i) goTo(r,regions[],b,i) end), vec(threshold, ), vec(), iden)
--regions[] = createBoxTriggerRegion("TriggerRegion", (function(r,b,f,i) goTo(r,regions[],b,i) end), vec(threshold, ), vec(), iden)


local function setInitialRegion(kart, id)
	if kart.Player.IsComputerControlled then
		kart.Player:CalculateNewWaypoint(nil, regions[0], nil)
	end
end

forEachKart(setInitialRegion)
]]