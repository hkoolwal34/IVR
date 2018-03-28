using System;
using System.Xml.Linq;

namespace _08_DTMF_Authentication
{
    class IVRFactory
    {

        public static ICommand CreateIVR(string ivrXml, Menu menu = null)
        {
            if (menu == null)
            {
                Console.WriteLine("check 1");
                var responseMenu = new Menu();

                try
                {

                    var xelement = XElement.Parse(ivrXml);
                    var menuElement = xelement.Element("menu");



                    if (menuElement != null)
                    {
                        InitElement(menuElement, responseMenu);
                        KeyElement(menuElement, responseMenu, false);
                    
                        return responseMenu;
                    }
                    return PlaySpeakElement(xelement);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Invalid ivr xml");
                }
            }
            else
            {
                Console.WriteLine("check 2");
                try
                {

                    var xelement = XElement.Parse(ivrXml);
                    var menuElement = xelement.Element("menu");

/*
                    var level = menuElement.Attribute("level");

                    int levelNumber;

                    int.TryParse(level.Value, out levelNumber);
                    Console.WriteLine("Level : ", levelNumber);
                    switch (levelNumber)
                    {
                        case 1:
                            Constants.ivrlevel1 = ivrXml;
                            break;
                        case 2:
                            Constants.ivrlevel2 = ivrXml;
                            break;
                    }
*/
                    if (menuElement == null)
                    {
                        PlaySpeakElement(xelement, menu);
                        return menu;
                    }

                    ForwardToUrl(menuElement, menu);
                    InitElement(menuElement, menu);
                    KeyElement(menuElement, menu, true);


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Invalid ivr xml");
                }

                return menu;
            }
            return null;
        }

        static void ForwardToUrl(XElement element, Menu menu)
        {
            var forwardToUrl = element.Attribute("forwardToUrl");

            if (forwardToUrl != null)
            {
                string[] strings = forwardToUrl.ToString().Split('"');
                menu.ForwardToUrl = strings[1];
            }
        }

        static ICommand PlaySpeakElement(XElement element)
        {
            var multipleCommand = new MultipleCommandHandler();
            foreach (var initElements in element.Elements())
            {
                switch (initElements.Name.ToString())
                {
                    case "play":
                        multipleCommand.AddCommand(new PlayCommand(initElements.Value.ToString()));
                        break;
                    case "speak":
                        multipleCommand.AddCommand(new SpeakCommand(initElements.Value.ToString()));
                        break;
                }
            }
            return multipleCommand;
        }

        static void PlaySpeakElement(XElement element, Menu menu)
        {
            foreach (var initElements in element.Elements())
            {
                switch (initElements.Name.ToString())
                {
                    case "play":
                        menu.AddInitCommand(new PlayCommand(initElements.Value));
                        break;
                    case "speak":
                        menu.AddInitCommand(new SpeakCommand(initElements.Value));
                        break;
                }
            }
        }

        static void InitElement(XElement element, Menu menu)
        {
            var menuInit = element.Element("init");

            if (menuInit == null)
            {
                Console.WriteLine("Wrong XML code!");
                return;
            }
            PlaySpeakElement(menuInit, menu);
        }

        static void KeyElement(XElement xelement, Menu menu, bool isInnerMenu)
        {
            var menuKeys = xelement.Element("keys");

            if (menuKeys == null)
            {
                Console.WriteLine("Missing <keys> node!");
                return;
            }

            foreach (var key in menuKeys.Elements("key"))
            {
                var pressedKeyAttribute = key.Attribute("pressed");

                if (pressedKeyAttribute == null)
                {
                    Console.WriteLine("Invalid ivr xml, keypress has no value!");
                    return;
                }

                int pressedKey;

                if (!int.TryParse(pressedKeyAttribute.Value, out pressedKey))
                {
                    Console.WriteLine("You did not add any number!");
                }

                foreach (var element in key.Elements())
                {
                    switch (element.Name.ToString())
                    {
                        case "play":
                            menu.AddKeypressCommand(pressedKey, new PlayCommand(element.Value));
                            break;
                        case "speak":
                            menu.AddKeypressCommand(pressedKey, new SpeakCommand(element.Value));
                            break;
                        case "menu":
                            if (isInnerMenu)
                            {
                                Menu innerMenu = new Menu();
                                menu.AddKeypressCommand(pressedKey, innerMenu);
                                CreateIVR(key.ToString(), innerMenu);
                            }
                            break;
/* 
 * V 1.0
 *                          case "previous":
                            if (Constants.level == 1)
                            {
                                Environment.Exit(0);
                            }
                            else if (Constants.level == 2)
                            {
                                var previouslevelXml = Constants.ivrlevel1;
                                var responseIvr = IVRFactory.CreateIVR(previouslevelXml, new Menu());
                                Constants.level--;
                                menu.StartCommand(responseIvr); 

                            }
                            break;
 */
                    }
                }
            }
        }

    }
}
