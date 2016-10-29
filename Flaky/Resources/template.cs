using System;
using Flaky;

namespace Flaky
{
public class Player : IPlayer
{
public override Source CreateSource()
{
var arp = new Arp(new Note[] { 0, 3, 7, 14, 17, 19 }, 0.5f + new Osc(0.2f, 0.4f), "arp");
var arp2 = new Arp(new Note[] { 17, 14, 7, 3, 0 }, 0.6f + new Osc(0.33f, 0.5f) + 2, "arp2");
var arp3 = new Arp(new Note[] { 0, 3, 7 }, 2f, "arp3");
var arp4 = new Arp(new Note[] { 5 }, 0.57f + new Osc(0.8f, 0.5f), "arp4");

var osc3 = new Osc(arp3 * 0.25f, 0.2f, "fm") * new AD(arp3, 1, 1);
var osc = new Osc(arp + osc3 * 800, 0.2f, "main") * new AD(arp, 0.01f, 0.09f);
Source osc2 = new Osc(arp2 + 800 * osc, 0.2f, "main2") * new AD(arp2, 0.01, 0.09f);

osc2 = new Pan(osc2, new Osc(10, 1));

for (int i = 1; i <= 5; i++)
{
  osc2 = new Delay(osc2, 0.1f * (float)i + 0.13f, $"d{i}");
}

var osc4 = new Osc(arp3 * 0.125f + 400 * osc3, 0.2f, "bass") * new AD(arp3, 1, 1);

var delay = new Delay(osc2 + osc4, 1.13f, "delay");
var delay2 = new Delay(delay, 5f, "delay2");

return delay2;
}
}
}