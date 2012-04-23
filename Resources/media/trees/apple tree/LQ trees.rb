#!/usr/bin/ruby1.9.1
require 'shellwords'
`ls *.muffin`.split("\n").each do |file|
  text = `cat #{file.shellescape}`
  text.gsub!(/AppleTreeLQ/) {|match| match + (1 + rand(8)).to_s}
  `echo #{text.shellescape} > #{file.shellescape}` 
end
