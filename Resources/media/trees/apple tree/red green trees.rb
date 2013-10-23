#!/usr/bin/ruby1.9.1
require 'shellwords'
`ls`.split("\n").each do |file|
  text = `cat #{file.shellescape}`
  text.gsub!(/Red/) {|match| if rand(2) == 1 then "Red" else "Green" end}
  `echo #{text.shellescape} > #{
file.shellescape}` 
end
