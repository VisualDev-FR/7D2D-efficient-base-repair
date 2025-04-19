from dataclasses import dataclass
from typing import List
import random


@dataclass
class Stack:
    size: int
    count: int
    added: int = 0


STACK_SIZE = 100

log_stream = open("ignore/python.log", "w")


def log(message: str):

    print(message)
    log_stream.write(message + "\n")


def compute(stacks: List[Stack], to_add: int) -> List[Stack]:

    datas = [(index, stack.count) for index, stack in enumerate(stacks)]

    while to_add > 0:

        available = [s for s in stacks if s.count < s.size]

        if not available:
            break

        min_count = min(stack.count for stack in available)

        min_level_stacks = [stack for stack in available if stack.count == min_count]

        total = len(min_level_stacks)

        if to_add >= total:
            for s in min_level_stacks:
                s.count += 1
            to_add -= total
        else:
            for s in min_level_stacks[:to_add]:
                s.count += 1
            to_add = 0

    # Log des changements
    for index, before in datas:
        after = stacks[index].count
        log(f"{before:<3} -> {after:<3} ({after - before:+})")
        # log(f"{before};{after};{after - before}")

    log(f"remaining: {to_add}\n")
    return stacks


if __name__ == "__main__":

    stacks = [Stack(size=100, count=60), Stack(size=100, count=80), Stack(size=100, count=20)]
    result = compute(stacks, 80)

    stacks = [Stack(size=100, count=60), Stack(size=100, count=80), Stack(size=100, count=20)]
    result = compute(stacks, 120)

    stacks = [Stack(size=100, count=100), Stack(size=100, count=100), Stack(size=100, count=100)]
    result = compute(stacks, 50)

    stacks = [Stack(size=100, count=60), Stack(size=100, count=90), Stack(size=100, count=40)]
    result = compute(stacks, 50)

    # stacks = [Stack(size=100, count=0) for _ in range(10_000)]  # 10 000 stacks vides
    # result = compute(stacks, 500_000)

    stacks = [Stack(size=100, count=random.randrange(0, 50)) for _ in range(10_000)]  # 10 000 stacks vides
    result = compute(stacks, 500_000)

    # stacks = [
    #     Stack(size=100, count=99) for _ in range(90_000)
    # ] + [
    #     Stack(size=100, count=10) for _ in range(10_000)
    # ]
    # result = compute(stacks, 1_000_000)
