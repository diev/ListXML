[Оглавление](index.md)

Параметры коммандной строки
===========================

Показ параметров с ключом `-h` или `--help`.
При желании перехватить вывод в файл: `ListXML -h 2>help.txt`

```
ListXML 6.2.61229.0
Copyright (c) Dmitrii Evdokimov 2009-2016

  -t, --test                                Test display of Settings and exit.

  -m admin@bank.ru, --mail=admin@bank.ru    Send a test mail to this address 
                                            and exit.

  -f, --force                               Force the rebuild of all files.

  -i, --ignore                              Ignore the statement exists.

  -p, --payments                            Extract payments only.

  -d YYYY-MM-DD, --date=YYYY-MM-DD          Specify a date to process those 
                                            files.

  -s HH, --switch=HH                        (Default: 11) Specify a hour to 
                                            switch between previous and current
                                            days.

  -v, --verbose                             Print verbose details during 
                                            execution.

  --help                                    Display this help screen.
```