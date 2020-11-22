// using System;
// using System.Collections.Generic;
// using System.Reactive;
// using System.Reactive.Linq;
// using System.Reactive.Subjects;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // ExceptionBubbles();
            // ExceptionBubbles2();
            // ExceptionBubbles3();
        }

        // private static void ExceptionBubbles()
        // {
        //     var behavingInterceptions = new List<string>();
        //     var misbehavingInterceptions = new List<string>();
        //
        //     var sut = new Subject<string>();
        //
        //     sut.Subscribe(v =>
        //     {
        //         Console.WriteLine($"S1:{v}");
        //         behavingInterceptions.Add(v);
        //     }, ex => { Console.WriteLine($"S1:{ex}"); });
        //
        //     sut.Subscribe(v =>
        //     {
        //         Console.WriteLine($"S2:{v}");
        //         misbehavingInterceptions.Add(v);
        //         throw new Exception(v);
        //     }, ex => { Console.WriteLine($"S2:{ex}"); });
        //
        //     sut.OnNext("v1");
        //     sut.OnNext("v2");
        //
        //     Console.WriteLine(behavingInterceptions.Count);
        //     Console.WriteLine(misbehavingInterceptions.Count);
        // }
        //
        // private static void ExceptionBubbles2()
        // {
        //     var behavingInterceptions = new List<string>();
        //     var misbehavingInterceptions = new List<string>();
        //
        //     var sut = new Subject<string>();
        //
        //     sut.Subscribe(v =>
        //     {
        //         Console.WriteLine($"S1:{v}");
        //         behavingInterceptions.Add(v);
        //     }, ex => { Console.WriteLine($"S1:{ex}"); });
        //
        //     sut.Catch<string, Exception>(ex =>
        //     {
        //         Console.WriteLine($"S2:{ex}");
        //         return new Subject<string>();
        //     }).Subscribe(v =>
        //     {
        //         Console.WriteLine($"S2:{v}");
        //         misbehavingInterceptions.Add(v);
        //         throw new Exception(v);
        //     }, ex => { Console.WriteLine($"S2:{ex}"); });
        //
        //     sut.OnNext("v1");
        //     sut.OnNext("v2");
        //
        //     Console.WriteLine(behavingInterceptions.Count);
        //     Console.WriteLine(misbehavingInterceptions.Count);
        // }
        //
        // private static void ExceptionBubbles3()
        // {
        //     var behavingInterceptions = new List<string>();
        //     var misbehavingInterceptions = new List<string>();
        //
        //     var sut = new Subject<string>();
        //
        //     sut.Subscribe(v =>
        //     {
        //         Console.WriteLine($"S1:{v}");
        //         behavingInterceptions.Add(v);
        //     }, ex => { Console.WriteLine($"S1:{ex}"); });
        //
        //     sut
        //         .Catch<string, Exception>(ex =>
        //         {
        //             Console.WriteLine($"S2:{ex}");
        //             return new Subject<string>();
        //         })
        //         .SubscribeSafe(new AnonymousObserver<string>(v =>
        //         {
        //             Console.WriteLine($"S2:{v}");
        //             misbehavingInterceptions.Add(v);
        //             throw new Exception(v);
        //         }, ex => { Console.WriteLine($"S2:{ex}"); }));
        //
        //     sut.OnNext("v1");
        //     sut.OnNext("v2");
        //
        //     Console.WriteLine(behavingInterceptions.Count);
        //     Console.WriteLine(misbehavingInterceptions.Count);
        // }
    }
}
