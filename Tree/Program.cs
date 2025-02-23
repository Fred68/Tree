
using Fred68.TreeItem;
using System.Collections.Specialized;
using Tree;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Tree class example");

TreeItem<Xxx> na = new TreeItem<Xxx>(new Xxx("Pippo"), null);

TreeItem<Xxx> nb1 = new TreeItem<Xxx>(new Xxx("Pluto"),na);
TreeItem<Xxx> nb2 = new TreeItem<Xxx>(new Xxx("Paperino"),na);

TreeItem<Xxx> nc1 = new TreeItem<Xxx>(new Xxx("Minnie"),nb1);
TreeItem<Xxx> nc2 = new TreeItem<Xxx>(new Xxx("Clarabella"),nb1);

TreeItem<Xxx> nd1 = new TreeItem<Xxx>(new Xxx("Qui"),nb2);
TreeItem<Xxx> nd2 = new TreeItem<Xxx>(new Xxx("Quo"),nb2);
TreeItem<Xxx> nd3 = new TreeItem<Xxx>(new Xxx("Qua"),nb2);

TreeItem<Xxx> ne1 = new TreeItem<Xxx>(new Xxx("Paperoga"),nd2);

TreeItem<Xxx> nf1 = new TreeItem<Xxx>(new Xxx("Zio Paperone"),ne1);
TreeItem<Xxx> nf2 = new TreeItem<Xxx>(new Xxx("Amelia"),ne1);

TreeItem<Xxx> ng1 = new TreeItem<Xxx>(new Xxx("Rockerduck"),nf1);

//foreach (Xxx x in na.Items(10))
foreach(TreeItem<Xxx> x in na.TreeItems())
{
	Console.WriteLine(x.ToString());
}

Console.ReadKey();
