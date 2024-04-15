using System.Collections.Generic;

[System.Serializable]
public struct Example1Parameters
{
    public string helloWorld;
    public int x;

    public Example1Parameters(string helloWorld, int x)
    {
        this.helloWorld = helloWorld;
        this.x = x;
    }
}

[System.Serializable]
public struct Example2Parameters
{
    public List<string> stringList;
    public int[] array;

    public Example2Parameters(List<string> stringList, int[] array)
    {
        this.stringList = stringList;
        this.array = array;
    }
}