FileStream fileStream = new FileStream("demo.eml", FileMode.Open, FileAccess.Read);
FileStream fout = new FileStream("demo.html", FileMode.Create, FileAccess.Write);
eml2html.Convert(fileStream, fout);
fileStream.Close();
fout.Close();
