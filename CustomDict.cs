using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDict
{
    class CustomDict<key_type, value_type>
    {
        public CustomDict<key_type, value_type> next { get; set; }      // Указатель на следующий элемент
        public key_type key { get; set; }                               // Ключ
        public value_type value { get; set; }                           // Значение
        private bool isFree = true;                                     // Свободна ли ячейка
        public bool Add(key_type key, value_type value)                 // Добавление элемента, для первого элемента
        {
            if (isFree)                                                  // Если ячейка свободна
            {
                this.isFree = false;
                this.key = key;
                this.value = value;
                return true;
            }
            if (this.key.Equals(key))                                   // Если ключ не уникален
                return false;
            if (this.next != null)                                       // Существует следующий элмент
                return this.next.Add(key, value, this);
            else                                                        // Следующий отсутствует, создаём и пишем в него
            {
                this.next = new CustomDict<key_type, value_type>();
                this.next.isFree = false;
                this.next.key = key;
                this.next.value = value;
                return true;
            }


        }
        // Добавление с сохранением родителя
        public bool Add(key_type key, value_type value, CustomDict<key_type, value_type> parent)
        {

            if (this.key.Equals(key))
                return false;
            if (isFree)
            {
                this.isFree = false;
                this.key = key;
                this.value = value;
            }
            else
            if (this.next != null)
                return this.next.Add(key, value, this);
            else
            {
                this.next = new CustomDict<key_type, value_type>();
                this.next.isFree = false;
                this.next.key = key;
                this.next.value = value;
                return true;
            }
            return true;

        }
        public dynamic Get(key_type key)                                // Получить элемент
        {
            if (this.key.Equals(key) && !this.isFree)                   // Ключи совпали, ячейка занята
                return this.value;
            else
                if (this.next != null)                                  // Рекурсивный поиск в списке
                    return this.next.Get(key);
                else
                    return null;
        }
        // Удаление элемента
        public void Remove(key_type key, CustomDict<key_type, value_type> parent)
        {
            if (this.key.Equals(key))                                   
            {
                if (this.next == null)                                  // Последный элемент
                {
                    if (parent != null)                                 // Не первый элемент
                    {
                        parent.next = null;
                        this.isFree = true;
                        return;
                    }
                    this.isFree = true;
                }
                else                                                    // Не последный элемент
                {
                    if(parent != null)                                  // Не первый элемент
                        parent.next = this.next;
                    else                                                // Первый элемент
                        this.isFree = true;
                }
            }
            else                                                        // Ключи различны, рекурсивный поиск
                this.next.Remove(key, this);
        }
        public bool ContainsKey(key_type key)                           // Содержит ли элемент
        {
            if (this.key.Equals(key) && !this.isFree)                   // Ключи совпали, ячейка занята
            {
                return true;
            }
            else                                                        // Рекурсивный поиск
            {
                if (this.next != null)
                    return this.next.ContainsKey(key);
            }
            return false;
        }

    }
}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CustomDict
//{
//    class CustomDict<key_type, value_type>
//    {
//        public CustomDict<key_type, value_type> previous { get; set; }  // Указатель на предыдущий элемент
//        public CustomDict<key_type, value_type> next { get; set; }      // Указатель на следующий элемент
//        public key_type key { get; set; }                               // Ключ
//        public value_type value { get; set; }                           // Значение
//        private long countElements = 0;                                 // Колличество последующих элеменов
//        private bool isFree = true;                                     // Свободна ли ячейка
//        public bool Add(key_type key, value_type value)                 // Добавление элемента, для первого элемента
//        {
//            countElements++;
//            if (isFree)                                                  // Если ячейка свободна
//            {
//                this.isFree = false;
//                this.key = key;
//                this.value = value;
//                this.next = new CustomDict<key_type, value_type>();
//                this.next.next = null;
//                this.next.previous = this;
//                return true;
//            }
//            else
//                if (this.key.Equals(key))                               // Если ключ не уникален
//            {
//                return false;
//            }
//            if (this.next != null)                                       // Существует следующий элмент
//            {
//                return this.next.Add(key, value, this);
//            }
//            else                                                        // Следующий отсутствует, создаём и пишем в него
//            {
//                this.next = new CustomDict<key_type, value_type>();
//                this.next.isFree = false;
//                this.next.key = key;
//                this.next.value = value;
//                this.next.previous = this;
//                return true;
//            }


//        }
//        // Добавление с сохранением родителя
//        public bool Add(key_type key, value_type value, CustomDict<key_type, value_type> parent)
//        {

//            if (this.key.Equals(key))
//                return false;
//            if (isFree)
//            {
//                this.isFree = false;
//                this.key = key;
//                this.value = value;
//            }
//            else
//            if (this.next != null)
//                return this.next.Add(key, value, this);
//            else
//            {
//                this.next = new CustomDict<key_type, value_type>();
//                this.next.isFree = false;
//                this.next.key = key;
//                this.next.value = value;
//                this.next.previous = this;
//                return true;
//            }
//            return true;

//        }
//        public dynamic Get(key_type key)
//        {
//            if (this.key.Equals(key) && !this.isFree)
//            {
//                return this.value;
//            }
//            else
//                if (this.next != null)
//            {
//                return this.next.Get(key);
//            }
//            else
//                return null;
//        }
//        public void Remove(key_type key)
//        {
//            if (this.key.Equals(key))
//            {
//                if (this.next == null && this.previous == null)
//                {
//                    countElements--;
//                    this.isFree = true;
//                }
//                else
//                    if (this.next == null || this.previous == null)
//                {
//                    if (this.next == null)
//                    {
//                        this.previous.next = null;
//                        countElements--;
//                        return;
//                    }
//                    else
//                    {
//                        this.isFree = true;
//                        countElements--;
//                        return;
//                    }
//                }
//                else
//                {
//                    countElements--;
//                    this.previous.next = this.next;
//                }
//                return;
//            }
//            this.next.Remove(key);
//            return;
//        }
//        public bool ContainsKey(key_type key)
//        {
//            if (this.key.Equals(key) && !this.isFree)
//            {
//                return true;
//            }
//            else
//            {
//                if (this.next != null)
//                {
//                    return this.next.ContainsKey(key);
//                }
//            }
//            return false;
//        }

//    }
//}
