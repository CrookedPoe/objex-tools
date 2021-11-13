using System;
using System.Text;

namespace animutil
{
    public class MyConsole
    {

        public static void WriteLine(string type, string fmt)
        {
            string symbol = String.Empty;
            ConsoleColor reset = Console.ForegroundColor;
            switch(type) {
                case "NOTE": {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[!] ");
                } break;
                case "PROMPT": {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[?] ");
                } break;
                case "SPEC": {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("[*] ");
                } break;
                case "ADD": {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[+] ");
                } break;
                case "DEL": {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[-] ");
                } break;
                case "WARN": {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[!] ");
                } break;
                case "ERR": {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[X] ");
                } break;
            }
            Console.ForegroundColor = reset;
            Console.Write($"{fmt}\n");
        }
    }
}